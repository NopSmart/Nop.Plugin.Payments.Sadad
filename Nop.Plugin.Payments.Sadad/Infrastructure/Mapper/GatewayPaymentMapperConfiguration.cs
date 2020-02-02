using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Payments.Sadad.Domain;
using Nop.Plugin.Payments.Sadad.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.Sadad.Infrastructure.Mapper
{
    public class GatewayPaymentMapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public GatewayPaymentMapperConfiguration()
        {
            //create specific maps
            CreateSadadGatewayPaymentsMaps();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Create sadad gateway payment maps 
        /// </summary>
        protected virtual void CreateSadadGatewayPaymentsMaps()
        {
            CreateMap<SadadGatewayPayment, SadadGatewayPaymentModel>();

        }
        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 10;

        #endregion
    }
}
