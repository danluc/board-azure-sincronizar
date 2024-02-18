using Back.Dominio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.EntityConfiguration
{
    public class ContaConfig : IEntityTypeConfiguration<Conta>
    {
        public void Configure(EntityTypeBuilder<Conta> builder)
        {
            builder.ToTable("CONTAS");
            builder.Property(p => p.Id).HasColumnType("Integer").IsRequired().ValueGeneratedOnAdd();
            builder.Property(p => p.Token).HasMaxLength(255).IsRequired();
            builder.Property(p => p.NomeUsuario).HasMaxLength(255);
            builder.Property(p => p.UrlCorporacao).HasMaxLength(255).IsRequired();
            builder.Property(p => p.ProjetoNome).HasMaxLength(255);
            builder.Property(p => p.ProjetoId).HasMaxLength(255);
            builder.Property(p => p.TimeNome).HasMaxLength(255);
            builder.Property(p => p.TimeId).HasMaxLength(255);
            builder.Property(p => p.ProjetoId).HasMaxLength(255).HasMaxLength(255);
            builder.Property(p => p.Principal).HasColumnType("Bit").IsRequired();
        }
    }
}