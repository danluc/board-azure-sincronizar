using Back.Data.Repository;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Microsoft.Extensions.DependencyInjection;

namespace APP.Configuracoes
{
    public static class ServicesRepositoryConfig
    {
        public static IServiceCollection AddRepositoryConfig(this IServiceCollection services)
        {
            #region CONSULTAS

            services.AddScoped<IRepositorioConsulta<Configuracao>, RepositorioConsulta<Configuracao>>();
            services.AddScoped<IRepositorioConsulta<Conta>, RepositorioConsulta<Conta>>();
            services.AddScoped<IRepositorioConsulta<Sincronizar>, RepositorioConsulta<Sincronizar>>();

            #endregion

            #region COMANDOS

            services.AddScoped<IRepositorioComando<Configuracao>, RepositorioComando<Configuracao>>();
            services.AddScoped<IRepositorioComando<Conta>, RepositorioComando<Conta>>();
            services.AddScoped<IRepositorioComando<Sincronizar>, RepositorioComando<Sincronizar>>();

            #endregion

            return services;
        }
    }
}
