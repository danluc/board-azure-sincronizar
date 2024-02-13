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

            #endregion

            #region COMANDOS

            services.AddScoped<IRepositorioComando<Configuracao>, RepositorioComando<Configuracao>>();
            services.AddScoped<IRepositorioComando<Conta>, RepositorioComando<Conta>>();

            #endregion

            return services;
        }
    }
}
