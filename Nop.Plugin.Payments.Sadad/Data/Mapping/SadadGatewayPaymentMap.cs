using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Payments.Sadad.Domain;

namespace Nop.Plugin.Payments.Sadad.Data.Mapping
{
    /// <summary>
    /// Represents a sadad gateway payment mapping configuration
    /// </summary>
    public class SadadGatewayPaymentMap : NopEntityTypeConfiguration<SadadGatewayPayment>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<SadadGatewayPayment> builder)
        {
            builder.ToTable(nameof(SadadGatewayPayment));
            builder.HasKey(sgp => sgp.Id);

            builder.Property(sgp => sgp.Amount).HasColumnType("decimal(18, 4)");
            builder.Property(sgp => sgp.Description).HasMaxLength(512);
            builder.Property(sgp => sgp.RequestKey).HasMaxLength(256);
            builder.Property(sgp => sgp.MerchantAdditionalData).HasMaxLength(128);
            builder.Property(sgp => sgp.TimeStamp).HasMaxLength(128);
            builder.Property(sgp => sgp.FingerPrint).HasMaxLength(128);
            builder.Property(sgp => sgp.RealTransactionDateTime).HasMaxLength(64);
            builder.Property(sgp => sgp.AppStatusDescription).HasMaxLength(128);
            builder.Property(sgp => sgp.RetrivalRefNo).HasMaxLength(128);
            builder.Property(sgp => sgp.ClientIPAddress).HasMaxLength(16).IsRequired();
            builder.Property(sgp => sgp.MerchantId).HasMaxLength(64).IsRequired().HasColumnType("varchar(64)");
            builder.Property(sgp => sgp.TerminalId).HasMaxLength(64).IsRequired().HasColumnType("varchar(64)");
            builder.Property(sgp => sgp.TransacionKey).HasMaxLength(64).IsRequired().HasColumnType("varchar(64)");

            base.Configure(builder);
        }

        #endregion
    }
}
