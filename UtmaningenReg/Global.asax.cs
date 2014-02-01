using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using UtmaningenReg.Helpers;

namespace UtmaningenReg
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure();
            log4net.ILog log = log4net.LogManager.GetLogger("Global.asax");
            log.Debug("Application started");
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown
            log4net.ILog log = log4net.LogManager.GetLogger("Global.asax");
            log.Debug("Application ended");
            log4net.LogManager.Shutdown();
        }


        void Application_Error(object sender, EventArgs e)
        {
            // Log unhandled exception
            log4net.ILog log = log4net.LogManager.GetLogger("Global.asax");
            Exception objErr = Server.GetLastError().GetBaseException();
            string err = "Error Caught in Application_Error event" +
                    "\nError in: " + Request.Url +
                    "\nError Message: " + objErr.Message;
            log.Error(err, objErr);

            try
            {
                if (!err.Contains("favicon.ico"))
                    SendMail.SendErrorMessage(err + "\n\n" + objErr.StackTrace);
            }
            catch (Exception exc)
            {
                log.Error("Unable to send error message to sysadm.", exc);
            }
        }

        protected void Application_EndRequest()
        {
            var exception = Server.GetLastError();

            if (exception != null)
            {
                var errorCode = Context.Response.StatusCode;
                log4net.ILog log = log4net.LogManager.GetLogger("Global.asax");
                log.Error(exception);
            }
        }
    }
}