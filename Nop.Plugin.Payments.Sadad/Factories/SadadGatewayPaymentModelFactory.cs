using Nop.Plugin.Payments.Sadad.Models;
using Nop.Plugin.Payments.Sadad.Services;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Services.Helpers;
using Nop.Plugin.Payments.Sadad.Extensions;

namespace Nop.Plugin.Payments.Sadad.Factories
{
    /// <summary>
    /// Represents the sadadGatewayPayment model factory
    /// </summary>
    public partial class SadadGatewayPaymentModelFactory : ISadadGatewayPaymentModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ISadadGatewayPaymentService _sadadGatewayPaymentService;

        #endregion

        #region Ctor

        public SadadGatewayPaymentModelFactory(IDateTimeHelper dateTimeHelper,
            ISadadGatewayPaymentService sadadGatewayPaymentService)
        {
            _dateTimeHelper = dateTimeHelper;
            _sadadGatewayPaymentService = sadadGatewayPaymentService;
        }

        #endregion

        #region Methods

        public SadadGatewayPaymentSearchModel PrepareGatewayPaymentSearchModel(SadadGatewayPaymentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public SadadGatewayPaymentListModel PrepareGatewayPaymentListModel(SadadGatewayPaymentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get sadadGatewayPayments
            var sadadGatewayPayments = _sadadGatewayPaymentService.GetSadadGatewayPayments(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new SadadGatewayPaymentListModel().PrepareToGrid(searchModel, sadadGatewayPayments, () =>
            {
                return sadadGatewayPayments.Select(sgp =>
                {
                    //fill in model values from the entity
                    var sadadGatewayPaymentModel = sgp.ToModel<SadadGatewayPaymentModel>();

                    //convert client ip address to string
                    sadadGatewayPaymentModel.ClientIPAddress = sgp.ClientIPAddress.ToStringIPAddress();

                    //convert dates to the user time
                    sadadGatewayPaymentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(sgp.CreatedOnUtc, DateTimeKind.Utc);
                    if (sgp.RequestedKeyOnUtc.HasValue)
                        sadadGatewayPaymentModel.RequestedKeyOn = _dateTimeHelper.ConvertToUserTime(sgp.RequestedKeyOnUtc.Value, DateTimeKind.Utc);
                    if (sgp.ReturnedFromBankSiteOnUtc.HasValue)
                        sadadGatewayPaymentModel.ReturnedFromBankSiteOn = _dateTimeHelper.ConvertToUserTime(sgp.ReturnedFromBankSiteOnUtc.Value, DateTimeKind.Utc);
                    if (sgp.PaymentVerificationOnUtc.HasValue)
                        sadadGatewayPaymentModel.PaymentVerificationOn = _dateTimeHelper.ConvertToUserTime(sgp.PaymentVerificationOnUtc.Value, DateTimeKind.Utc);

                    return sadadGatewayPaymentModel;
                });
            });

            return model;
        }

        #endregion
    }
}
