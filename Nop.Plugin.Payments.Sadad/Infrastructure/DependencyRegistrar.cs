using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.Sadad.Data;
using Nop.Plugin.Payments.Sadad.Domain;
using Nop.Plugin.Payments.Sadad.Factories;
using Nop.Plugin.Payments.Sadad.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Payments.Sadad.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "sc_object_context_nopsmart_IrPayment_Sadad";

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            ContainerBuilderExtensions.RegisterPluginDataContext<PaymentObjectContext>(builder, CONTEXT_NAME);
            builder.RegisterType<SadadGatewayPaymentService>().As<ISadadGatewayPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<EfRepository<SadadGatewayPayment>>().As<IRepository<SadadGatewayPayment>>().WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME)).InstancePerLifetimeScope();
            
            //payment factories
            builder.RegisterType<SadadGatewayPaymentModelFactory>().As<ISadadGatewayPaymentModelFactory>().InstancePerLifetimeScope();
        }

        public int Order
        {
            get
            {
                return 4;
            }
        }
    }
}
