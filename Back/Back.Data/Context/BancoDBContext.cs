using Back.Data.EntityConfiguration;
using Back.Dominio.Models;
using Microsoft.EntityFrameworkCore;

namespace Back.Data.Context
{
    public class BancoDBContext : DbContext
    {
        public BancoDBContext(DbContextOptions<BancoDBContext> options) : base(options)
        {
        }

        public virtual DbSet<Configuracao> Configuracoes { get; set; }
        public virtual DbSet<Conta> Contas { get; set; }
        public virtual DbSet<Sincronizar> Sincronizacoes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("DataSource=local.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ConfiguracaoConfig());
            modelBuilder.ApplyConfiguration(new ContaConfig());
            modelBuilder.ApplyConfiguration(new SincronizarConfig());
        }
    }
}
