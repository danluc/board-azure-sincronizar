using Back.Dominio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.EntityConfiguration
{
    public class SincronizarConfig : IEntityTypeConfiguration<Sincronizar>
    {
        public void Configure(EntityTypeBuilder<Sincronizar> builder)
        {
            builder.ToTable("SINCRONIZACOES");
            builder.Property(p => p.Id).HasColumnType("Integer").IsRequired().ValueGeneratedOnAdd();
            builder.Property(p => p.DataInicio).HasColumnType("DateTime").IsRequired();
            builder.Property(p => p.DataInicio).HasColumnType("DateTime");
            builder.Property(p => p.Status).HasColumnType("Int");
        }
    }
}