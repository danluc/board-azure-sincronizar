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
            builder.Property(p => p.AreaId).HasColumnType("Integer");
            builder.Property(p => p.SprintId).HasColumnType("Integer");
            builder.Property(p => p.Ativo).HasColumnType("Bit");
            builder.Property(p => p.EmailDe).HasMaxLength(255);
            builder.Property(p => p.EmailPara).HasMaxLength(255);
            builder.Property(p => p.AreaPath).HasMaxLength(255);
            builder.Property(p => p.Sprint).HasMaxLength(255);
            builder.Property(p => p.Cliente).HasMaxLength(255);
            builder.Property(p => p.Sprint).HasMaxLength(255);
        }
    }
}