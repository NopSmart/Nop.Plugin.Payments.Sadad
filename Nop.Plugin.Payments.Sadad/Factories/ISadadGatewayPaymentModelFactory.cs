using Nop.Plugin.Payments.Sadad.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.Sadad.Factories
{
    /// <summary>
    /// Represents the sadadGatewayPayment model factory
    /// </summary>
    public partial interface ISadadGatewayPaymentModelFactory
    {
        SadadGatewayPaymentSearchModel PrepareGatewayPaymentSearchModel(SadadGatewayPaymentSearchModel searchModel);

        SadadGatewayPaymentListModel PrepareGatewayPaymentListModel(SadadGatewayPaymentSearchModel searchModel);
    }
}
