using Back.Dominio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.EntityConfiguration
{
    public class ConfiguracaoConfig : IEntityTypeConfiguration<Configuracao>
    {
        public void Configure(EntityTypeBuilder<Configuracao> builder)
        {
            builder.ToTable("CONFIGURACOES");
            builder.Property(p => p.Id).HasColumnType("Integer").IsRequired().ValueGeneratedOnAdd();
            builder.Property(p => p.Dia).HasColumnType("Int").IsRequired();
            builder.Property(p => p.HoraCron).HasMaxLength(10).IsRequired();
            builder.Property(p => p.Cliente).HasMaxLength(255);
        }
    }
}