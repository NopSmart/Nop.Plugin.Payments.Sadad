using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.Sadad.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Sadad.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Sadad.Fields.TerminalId")]
        public string TerminalId { get; set; }
        public bool TerminalId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Sadad.Fields.TransacionKey")]
        public string TransacionKey { get; set; }
        public bool TransacionKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Sadad.Fields.CurrencyIsToman")]
        public bool CurrencyIsToman { get; set; }
        public bool CurrencyIsToman_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Sadad.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Sadad.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}