using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.Sadad
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            //Redirect To Bank
            routeBuilder.MapRoute("Plugin.Payments.Sadad.RedirectToBank", "Plugins/PaymentSadad/RedirectToBank/{id?}",
                 new { controller = "PaymentSadad", action = "RedirectToBank" });

            //Verify
            routeBuilder.MapRoute("Plugin.Payments.Sadad.Verify", "Plugins/PaymentSadad/Verify/{id?}",
                 new { controller = "PaymentSadad", action = "Verify" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority
        {
            get { return -1; }
        }
    }
}
