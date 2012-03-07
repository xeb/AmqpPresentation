using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance
{
    public class MvcApplication : System.Web.HttpApplication
    {
        // ReSharper disable InconsistentNaming
        public static string RabbitMQBaseAddress = "http://localhost:1339/PerfTestRabbit";
        // ReSharper restore InconsistentNaming
        
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("handler.ashx");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        // ReSharper disable InconsistentNaming
        protected void Application_Start()
        // ReSharper restore InconsistentNaming
        {
            var serviceHost = new ServiceHost(typeof(DuplexRabbitService), new Uri(RabbitMQBaseAddress));
            serviceHost.Open();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Stop()
        {
        }
    }
}