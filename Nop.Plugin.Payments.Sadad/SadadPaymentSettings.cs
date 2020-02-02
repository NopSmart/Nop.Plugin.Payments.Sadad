using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Sadad
{
    /// <summary>
    /// Represents settings of the Sadad payment plugin
    /// </summary>
    public class SadadPaymentSettings : ISettings
    {
        public bool CurrencyIsToman { get; set; }
        //public bool SaveTransactionLogs { get; set; }

        public string MerchantId { get; set; }

        public string TerminalId { get; set; }

        public string TransacionKey { get; set; }

        //public long OrderId { get; set; }

        /// <summary>
        /// Gets or sets an additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}
