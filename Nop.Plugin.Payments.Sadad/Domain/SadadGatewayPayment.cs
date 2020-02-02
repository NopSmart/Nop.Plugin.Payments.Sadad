using Nop.Core;
using System;

namespace Nop.Plugin.Payments.Sadad.Domain
{
    public class SadadGatewayPayment : BaseEntity
    {
        public decimal Amount { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? RequestedKeyOnUtc { get; set; }
        public DateTime? PaymentVerificationOnUtc { get; set; }
        public DateTime? ReturnedFromBankSiteOnUtc { get; set; }
        public string Description { get; set; }
        public string RequestKey { get; set; }
        public string MerchantAdditionalData { get; set; }
        public string TimeStamp { get; set; }
        public string FingerPrint { get; set; }
        public string RealTransactionDateTime { get; set; }
        public short? AppStatusCode { get; set; }
        public string AppStatusDescription { get; set; }
        public string RetrivalRefNo { get; set; }
        public byte[] ClientIPAddress { get; set; }
        public int OrderId { get; set; }
        public string MerchantId { get; set; }
        public string TerminalId { get; set; }
        public string TransacionKey { get; set; }
        public bool PaymentSucceed { get; set; }
    }
}
