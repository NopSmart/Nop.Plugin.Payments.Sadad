using Nop.Core;
using Nop.Plugin.Payments.Sadad.Domain;

namespace Nop.Plugin.Payments.Sadad.Services
{
    public interface ISadadGatewayPaymentService
    {
        void Insert(SadadGatewayPayment item);

        void Update(SadadGatewayPayment item);

        SadadGatewayPayment GetSadadGatewayPaymentById(int Id);

        IPagedList<SadadGatewayPayment> GetSadadGatewayPayments(int pageIndex = 0, int pageSize = 2147483647);
    }
}
