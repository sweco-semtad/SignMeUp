using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using log4net;
using PaysonIntegration;
using PaysonIntegration.Data;
using PaysonIntegration.Utils;
using UtmaningenReg.Helpers;
using UtmaningenReg.Models;

namespace UtmaningenReg.Controllers
{
    public class CheckoutController : RegBaseController
    {
        // For testing only
        private const string ApplicationId = "Utmaningen anmälan";

        private readonly ILog log;

        public CheckoutController()
        {
            log = LogManager.GetLogger(GetType());
        }

        //
        // GET: /Checkout/
        public ActionResult Checkout(string cancel, string betala)
        {
            log.Debug("Checkout");

            if (!string.IsNullOrEmpty(betala))
            {
                return RedirectToAction("Pay");
            }
            if (!string.IsNullOrEmpty(cancel))
            {
                log.Debug("Checkout, cancel");
                return RedirectToAction("Create", "Home");
            }

            var checkout = (CheckoutModel)Session["checkout"];
            log.Debug("Checkout, lagnamn: " + checkout.Registrering.Lagnamn);
            return View(checkout);
        }

        public ActionResult Pay()
        {
            try
            {
                var checkout = (CheckoutModel)Session["checkout"];

                if (checkout == null || checkout.Registrering == null)
                {
                    return ShowError("Missing data, checkout or checkout.Registrering is null at Pay()");
                }

                log.Debug("Checkout, pay. Lagnamn: " + checkout.Registrering.Lagnamn);

                SaveNewRegistration(checkout.Registrering);
                FillRegistrering(checkout.Registrering);

                checkout.RegId = checkout.Registrering.ID;

                // We remove port info to help when the site is behind a load balancer/firewall that does port rewrites.
                var scheme = Request.Url.Scheme;
                var host = Request.Url.Host;
                //var oldPort = Request.Url.Port.ToString();
                var returnUrl = Url.Action("Returned", "Checkout", new RouteValueDictionary(), scheme, host)/*.Replace(oldPort, "")*/ + "?regId=" + checkout.RegId;

                var cancelUrl = Url.Action("Cancelled", "Checkout", new RouteValueDictionary(), scheme, host)/*.Replace(oldPort, "")*/;

                var sender = new Sender(checkout.SenderEmail);
                sender.FirstName = checkout.SenderFirstName;
                sender.LastName = checkout.SenderLastName;

                var totalAmount = Avgift.Kalk(checkout.Registrering);

                var receiver = new Receiver(CheckoutModel.PaysonRecieverEmail, totalAmount);
                receiver.FirstName = CheckoutModel.PaysonRecieverFirstName;
                receiver.LastName = CheckoutModel.PaysonRecieverLastName;
                receiver.SetPrimaryReceiver(true);

                var payData = new PayData(returnUrl, cancelUrl, "Utmaningen 2014 - " + checkout.Registrering.Lagnamn, sender, new List<Receiver> { receiver });

                // Set IPN callback URL
                // When the shop is hosted by Payson the IPN scheme must be http and not https
                var ipnNotificationUrl = Url.Action("IPN", "Checkout", new RouteValueDictionary(), scheme, host)/*.Replace(oldPort, "")*/ + "?regId=" + checkout.RegId;
                payData.SetIpnNotificationUrl(ipnNotificationUrl);

                payData.SetFundingConstraints(new List<FundingConstraint> { FundingConstraint.Bank, FundingConstraint.CreditCard });
                payData.SetTrackingId(checkout.Registrering.ID.ToString());

                var orderItems = new List<PaysonIntegration.Utils.OrderItem>();
                var reg = checkout.Registrering;
                // Lägg in värden på kvitto
                var oi1 = new PaysonIntegration.Utils.OrderItem("Utmaningen " + DateTime.Now.Year + ", bana " + reg.Banor.Namn);                    
                oi1.SetOptionalParameters("st", 1, reg.Banor.Avgift, 0);
                orderItems.Add(oi1);
                if (reg.Kanoter.Avgift != 0)
                {
                    var oi2 = new PaysonIntegration.Utils.OrderItem("Kanot, " + reg.Kanoter.Namn);
                    oi2.SetOptionalParameters("st", 1, (decimal)reg.Kanoter.Avgift, 0);
                    orderItems.Add(oi2);
                }
                if (reg.Forseningsavgift != 0)
                {
                    var oi3 = new PaysonIntegration.Utils.OrderItem("Avgift för sen anmälan");
                    oi3.SetOptionalParameters("st", 1, (decimal)reg.Forseningsavgift, 0);
                    orderItems.Add(oi3);
                }
                if (reg.Rabatt != 0)
                {
                    var oi4 = new PaysonIntegration.Utils.OrderItem("Rabatt");
                    oi4.SetOptionalParameters("st", 1, -(decimal)reg.Rabatt, 0);
                    orderItems.Add(oi4);
                }
                payData.SetOrderItems(orderItems);

                var api = new PaysonApi(CheckoutModel.PaysonUserId, CheckoutModel.PaysonUserKey, ApplicationId, false);
    #if DEBUG
                api = new PaysonApi("4", "2acab30d-fe50-426f-90d7-8c60a7eb31d4", ApplicationId, true);
    #endif

                var response = api.MakePayRequest(payData);

                if (response.Success)
                {
                    checkout.Token = response.Token;
                    checkout.Registrering.PaysonToken = response.Token;
                    SaveChanges(checkout.Registrering);

                    var forwardUrl = api.GetForwardPayUrl(response.Token);
                                        
                    Session["checkout"] = checkout;

                    return Redirect(forwardUrl);
                }

                return ShowPaymentError("Error when sending payment to payson.", response.NvpContent, checkout.Registrering);
            }
            catch (Exception exception)
            {
                log.Error("Exception in pay.", exception);
                return ShowError("Error in Pay().", exception);
            }
        }


        public ActionResult Returned(string regId)
        {   
            int registrationId = -1;

            log.Debug("Returned");

            if (!string.IsNullOrEmpty(regId) && !int.TryParse(regId, out registrationId))
            {
                ShowError("regId not in querystring at checkout returned.");
            }

            var registration = db.Registreringar.FirstOrDefault(regg => regg.ID == registrationId);

            if (registration == null)
            {
                ShowError("No registration found in db with id: " + registrationId + " in checkout returned.");
            }

            log.Debug("Returned. Lagnamn: " + registration.Lagnamn);

            // If no payment message has been sent (IPN)
            if (!registration.HarBetalt)
            {
                var api = new PaysonApi(CheckoutModel.PaysonUserId, CheckoutModel.PaysonUserKey);
#if DEBUG
                api = new PaysonApi("4", "2acab30d-fe50-426f-90d7-8c60a7eb31d4", ApplicationId, true);
#endif
                var response = api.MakePaymentDetailsRequest(new PaymentDetailsData(registration.PaysonToken));

                if (response.Success && (response.PaymentDetails.PaymentStatus == PaymentStatus.Completed ||
                                        response.PaymentDetails.PaymentStatus == PaymentStatus.Pending))
                {
                    if (!registration.HarBetalt)
                    {
                        SetAsPaid(registration);
                    }
                }
                else
                {
                    log.Warn("Deleting temp-registration with id: " + registrationId);
                    // Remove the temporary registration
                    DeleteRegistrering(registrationId);

                    return ShowPaymentError("Error when payment returned.", response.NvpContent, registration);
                }
            }

            return RedirectToAction("Redirect", "Home");
        }

        public ActionResult Cancelled()
        {
            return RedirectToAction("Create", "Home");
        }

        public ActionResult IPN(string regId)
        {
            log.Debug("IPN regId: " + regId);

            int regIdInt = -1;
            int.TryParse(regId, out regIdInt);

            var registration = db.Registreringar.FirstOrDefault(regg => regg.ID == regIdInt);

            if (registration != null)
            {
                Request.InputStream.Position = 0;
                var content = new StreamReader(Request.InputStream).ReadToEnd();

                var api = new PaysonApi(CheckoutModel.PaysonUserId, CheckoutModel.PaysonUserKey, ApplicationId, true);
                var response = api.MakeValidateIpnContentRequest(content);
                var statusText = response.ProcessedIpnMessage.PaymentStatus.HasValue
                                    ? response.ProcessedIpnMessage.PaymentStatus.ToString()
                                    : "N/A";
                var status = response.ProcessedIpnMessage.PaymentStatus;

                log.Debug("IPN message content: " + response.Content);
                log.Debug("IPN raw response: " + content);

                if (status == PaymentStatus.Completed || status == PaymentStatus.Completed)
                {
                    log.Debug("IPN message, status: " + statusText + ". regId: " + regId + " success: " + response.Success);

                    if (!registration.HarBetalt)
                    {
                        SetAsPaid(registration);
                    }
                }
                else
                {
                    log.Debug("IPN message for non complete transaction. regId: " + regId + ". Status: " + statusText);
                }
            }
            else
            {
                log.Error("Got IPN with wrong regId as query parameter: " + regId);
                SendMail.SendErrorMessage("Got IPN with wrong regId as query parameter: " + regId);
            }

            return new EmptyResult();
        }

        public ActionResult Invoice()
        {   
            return View();
        }

        [HttpPost]
        public ActionResult Invoice(Invoice invoice, string andra, string betala)
        {
            try
            {
                log.Debug("Invoice");

                if (!string.IsNullOrEmpty(andra))
                {
                    return RedirectToAction("Create", "Home");
                }

                var reg = (Registreringar) Session["reg"];
                reg.Invoice = invoice;
                // Save to db
                SaveNewRegistration(reg);

                try
                {
                    // Send acceptance mail
                    FillViewData();
                    var appUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
                    var link = appUrl + "home/mail/" + reg.ID;
                    SendMail.SendRegistration(RenderRazorViewToString("_MailView", reg), appUrl, link, reg.Epost);
                    log.Debug("Sent confirmation mail on invoice. Lagnamn: " + reg.Lagnamn);
                }
                catch (Exception exception)
                {
                    log.Error("Error when sending acceptance mail for invoice.", exception);
                    try
                    {
                        SendMail.SendErrorMessage("Error when sending acceptance mail for invoice.\n\n" + exception.Message + "\n\n" + exception.StackTrace);
                    }
                    catch {}
                }

                return RedirectToAction("Redirect", "Home");
            }
            catch (Exception exception)
            {
                return ShowError("Error when saving an invoice.", exception);
            }
        }

        public ActionResult ShowPaymentError(string logMessage, IDictionary<string, string> response, Registreringar registration)
        {
            var str = new StringBuilder();
            str.Append(logMessage);
            str.Append("\n");

            foreach (KeyValuePair<string, string> error in response)
            {
                str.Append(error.Key + ": " + error.Value);
                str.Append("\n");
            }
            log.Error(str.ToString());

            try
            {
                SendMail.SendErrorMessage(str.ToString());
            }
            catch (Exception exception)
            {
                log.Error("Erro when sending error message.", exception);
            }

            if (response.ContainsKey("errorList.error(0).message"))
            {
                TempData["PaymentErrorMessage"] = response["errorList.error(0).message"];
                TempData["PaymentErrorParameter"] = response["errorList.error(0).parameter"];
            }
            else
            {
                TempData["PaymentErrorMessage"] = "Betalningen avbröts av okänd anledning.";
                TempData["PaymentErrorParameter"] = "Okänd";
            }

            return RedirectToAction("Create", "Home", registration);
        }
    }
}
