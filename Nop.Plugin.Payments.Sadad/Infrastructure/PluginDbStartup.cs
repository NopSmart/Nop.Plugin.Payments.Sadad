using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.Sadad.Data;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Payments.Sadad.Infrastructure
{
    public class PluginDbStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PaymentObjectContext>(optionsBuilder => DbContextOptionsBuilderExtensions.UseSqlServerWithLazyLoading(optionsBuilder, services), ServiceLifetime.Scoped, ServiceLifetime.Scoped);
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order
        {
            get
            {
                return 11;
            }
        }
    }
}
