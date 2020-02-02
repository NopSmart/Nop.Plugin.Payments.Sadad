using Nop.Core;
using Nop.Core.Data;
using Nop.Plugin.Payments.Sadad.Domain;
using Nop.Services.Events;
using System.Linq;

namespace Nop.Plugin.Payments.Sadad.Services
{
    public class SadadGatewayPaymentService : ISadadGatewayPaymentService
    {
        #region Field
        private readonly IRepository<SadadGatewayPayment> _sadadGatewayPaymentRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctr

        public SadadGatewayPaymentService(IRepository<SadadGatewayPayment> sadadGatewayPaymentRepository, IEventPublisher eventPublisher)
        {
            this._sadadGatewayPaymentRepository = sadadGatewayPaymentRepository;
            this._eventPublisher = eventPublisher;

        }

        #endregion

        #region Methods

        public void Insert(SadadGatewayPayment item)
        {
            _sadadGatewayPaymentRepository.Insert(item);
        }

        public SadadGatewayPayment GetSadadGatewayPaymentById(int Id)
        {
            return _sadadGatewayPaymentRepository.GetById(Id);
        }

        public void Update(SadadGatewayPayment item)
        {
            _sadadGatewayPaymentRepository.Update(item);
        }

        public IPagedList<SadadGatewayPayment> GetSadadGatewayPayments(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _sadadGatewayPaymentRepository.Table;
            query = from c in _sadadGatewayPaymentRepository.Table
                    select c;


            query = query.OrderBy(p => p.Id);

            var sadadGatewayPayments = new PagedList<SadadGatewayPayment>(query, pageIndex, pageSize);
            return sadadGatewayPayments;
        }
        #endregion
    }
}
