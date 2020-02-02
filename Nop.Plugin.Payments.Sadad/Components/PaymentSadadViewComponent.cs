using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.Sadad.Components
{
    [ViewComponent(Name = "PaymentSadad")]
    public class PaymentSadadViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.Sadad/Views/PaymentInfo.cshtml");
        }
    }
}
