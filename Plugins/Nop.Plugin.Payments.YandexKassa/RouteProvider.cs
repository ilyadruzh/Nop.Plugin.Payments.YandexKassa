using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.YandexKassa
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.YandexKassa.Configure", "Plugins/PaymentYandexKassa/Configure",
                new { controller = "PaymentYandexKassa", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.YandexKassa.Controllers" }
            );

            //Return
            routes.MapRoute("Plugin.Payments.YandexKassa.Return",
                 "Plugins/PaymentYandexKassa/Return",
                 new { controller = "PaymentYandexKassa", action = "Return" },
                 new[] { "Nop.Plugin.Payments.YandexKassa.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
