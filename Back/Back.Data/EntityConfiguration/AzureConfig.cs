using Back.Dominio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.EntityConfiguration
{
    public class AzureConfig : IEntityTypeConfiguration<Azure>
    {
        public void Configure(EntityTypeBuilder<Azure> builder)
        {
            builder.ToTable("AZURE");
            builder.Property(p => p.Id).HasColumnType("Integer").IsRequired().ValueGeneratedOnAdd();
            builder.Property(p => p.UrlCorporacao).HasMaxLength(255);
            builder.Property(p => p.Token).HasMaxLength(255);
            builder.Property(p => p.ProjetoNome).HasMaxLength(255);
            builder.Property(p => p.ProjetoId).HasMaxLength(255);
            builder.Property(p => p.TimeNome).HasMaxLength(255);
            builder.Property(p => p.TimeId).HasMaxLength(255);
        }
    }
}