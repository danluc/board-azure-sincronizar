using Back.Dominio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.EntityConfiguration
{
    public class SincronizarItemConfig : IEntityTypeConfiguration<SincronizarItem>
    {
        public void Configure(EntityTypeBuilder<SincronizarItem> builder)
        {
            builder.ToTable("SINCRONIZAR_ITENS");
            builder.Property(p => p.Id).HasColumnType("Integer").IsRequired().ValueGeneratedOnAdd();
            builder.Property(p => p.SincronizarId).HasColumnType("Integer").IsRequired();
            builder.Property(p => p.Destino).HasMaxLength(100);
            builder.Property(p => p.Origem).HasMaxLength(100);
            builder.Property(p => p.Status).HasMaxLength(100);
            builder.Property(p => p.Tipo).HasMaxLength(100);
        }
    }
}
