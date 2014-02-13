using System;
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
        private const string ApplicationId = "Payson Demo WebShop 1.0";

        private readonly ILog log;

        public CheckoutController()
        {
            log = LogManager.GetLogger(GetType());
        }

        //
        // GET: /Checkout/
        public ActionResult Checkout(string cancel, string betala)
        {
            if (!string.IsNullOrEmpty(betala))
            {
                return RedirectToAction("Pay");
            }
            if (!string.IsNullOrEmpty(cancel))
            {
                return RedirectToAction("Create", "Home");
            }

            var checkout = (CheckoutModel)Session["checkout"];
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

                SaveNewRegistration(checkout.Registrering);
                FillRegistrering(checkout.Registrering);

                checkout.RegId = checkout.Registrering.ID;

                // We remove port info to help when the site is behind a load balancer/firewall that does port rewrites.
                var scheme = Request.Url.Scheme;
                var host = Request.Url.Host;
                //var oldPort = Request.Url.Port.ToString();
                var returnUrl = Url.Action("Returned", "Checkout", new RouteValueDictionary(), scheme, host)/*.Replace(oldPort, "")*/ + "?reg=" + checkout.RegId;

                var cancelUrl = Url.Action("Cancelled", "Checkout", new RouteValueDictionary(), scheme, host)/*.Replace(oldPort, "")*/;

                // When the shop is hosted by Payson the IPN scheme must be http and not https
                //var ipnNotificationUrl = Url.Action("IPN", "Checkout", new RouteValueDictionary(), "http", host)/*.Replace(oldPort, "")*/ + "?reg=" + checkout.RegId;

                var sender = new Sender(checkout.SenderEmail);
                sender.FirstName = checkout.SenderFirstName;
                sender.LastName = checkout.SenderLastName;

                var totalAmount = Avgift.Kalk(checkout.Registrering);

                var receiver = new Receiver(CheckoutModel.PaysonRecieverEmail, totalAmount);
                receiver.FirstName = CheckoutModel.PaysonRecieverFirstName;
                receiver.LastName = CheckoutModel.PaysonRecieverLastName;
                receiver.SetPrimaryReceiver(true);

                var payData = new PayData(returnUrl, cancelUrl, "Utmaningen 2013 - " + checkout.Registrering.Lagnamn, sender, new List<Receiver> { receiver });

                var fundingConstraints = new List<FundingConstraint> { FundingConstraint.Bank, FundingConstraint.CreditCard };

                payData.SetFundingConstraints(fundingConstraints);

                payData.SetTrackingId(checkout.Registrering.ID.ToString());

                var api = new PaysonApi(CheckoutModel.PaysonUserId, CheckoutModel.PaysonUserKey, null);
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

                    return Redirect(forwardUrl);
                }

                return ShowPaymentError("Error when sending payment to payson.", response.NvpContent, checkout.Registrering);

            }
            catch (Exception exception)
            {
                return ShowError("Error in Pay().", exception);
            }
        }


        public ActionResult Returned()
        {
            var regId = Request.QueryString["reg"];
            int registrationId = -1;

            if (!string.IsNullOrEmpty(regId) && !int.TryParse(regId, out registrationId))
            {
                ShowError("regId not in querystring at checkout returned.");
            }

            var registration = db.Registreringar.FirstOrDefault(reg => reg.ID == registrationId);

            if (registration == null)
            {
                ShowError("No registration found in db with id: " + registrationId + " in checkout returned.");
            }

            var api = new PaysonApi(CheckoutModel.PaysonUserId, CheckoutModel.PaysonUserKey, null);
#if DEBUG
            api = new PaysonApi("4", "2acab30d-fe50-426f-90d7-8c60a7eb31d4", ApplicationId, true);
#endif
            var response = api.MakePaymentDetailsRequest(new PaymentDetailsData(registration.PaysonToken));

            if (response.Success && (response.PaymentDetails.PaymentStatus == PaymentStatus.Completed ||
                                    response.PaymentDetails.PaymentStatus == PaymentStatus.Pending))
            {
                registration.HarBetalt = true;
                SaveChanges(registration);

                try
                {
                    FillViewData();
                    var appUrl = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority, Url.Content("~"));
                    var link = appUrl + "home/mail/" + registration.ID;
                    SendMail.SendRegistration(RenderRazorViewToString("_MailView", registration), appUrl, link, registration.Epost);
                }
                catch (Exception exc)
                {
                    return ShowError("Unable to send confirmation mail", exc);
                }
            }
            else
            {
                // Remove the temporary registration
                DeleteRegistrering(registrationId);

                return ShowPaymentError("Error when payment returned.", response.NvpContent, registration);
            }

            return RedirectToAction("Redirect", "Home");
        }

        public ActionResult Cancelled()
        {
            return RedirectToAction("Create", "Home");
        }

        //public ActionResult IPN(Guid regGuid)
        //{
        //    Request.InputStream.Position = 0;
        //    var content = new StreamReader(Request.InputStream).ReadToEnd();

        //    var checkout = _repository.GetCheckout(regGuid);

        //    if (checkout != null)
        //    {
        //        var api = new PaysonApi(CheckoutModel.PaysonUserID, CheckoutModel.PaysonUserKey, ApplicationId, true);
        //        var response = api.MakeValidateIpnContentRequest(content);
        //        if (response.Success)
        //        {
        //            var status = response.ProcessedIpnMessage.PaymentStatus.HasValue
        //                             ? response.ProcessedIpnMessage.PaymentStatus.ToString()
        //                             : "N/A";
        //            //checkout.Updates[DateTime.Now] = "IPN: " + status;
        //            //checkout.LatestStatus = status;
        //        }
        //        else
        //        {
        //            //checkout.Updates[DateTime.Now] = "IPN: IPN Failure";
        //            //checkout.LatestStatus = "Failure";
        //        }
        //    }

        //    return new EmptyResult();
        //}

        public ActionResult Invoice()
        {   
            return View();
        }

        [HttpPost]
        public ActionResult Invoice(Invoice invoice, string andra, string betala)
        {
            try
            {
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
                }
                catch (Exception exception)
                {
                    log.Error("Error when sending acceptance mail for invoice.", exception);
                    SendMail.SendErrorMessage("Error when sending acceptance mail for invoice.\n\n" + exception.Message + "\n\n" + exception.StackTrace);
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
