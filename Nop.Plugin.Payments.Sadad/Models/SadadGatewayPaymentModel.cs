using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.Sadad.Models
{
    /// <summary>
    /// Represents a SadadGatewayPaymentModel model
    /// </summary>
    public partial class SadadGatewayPaymentModel : BaseNopEntityModel
    {
        #region Properties
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? RequestedKeyOn { get; set; }
        public DateTime? PaymentVerificationOn { get; set; }
        public DateTime? ReturnedFromBankSiteOn { get; set; }
        public short? AppStatusCode { get; set; }
        public string AppStatusDescription { get; set; }
        public string ClientIPAddress { get; set; }
        public int OrderId { get; set; }
        public bool PaymentSucceed { get; set; }
        #endregion
    }
}
