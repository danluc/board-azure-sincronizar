using APP.Configuracoes.Cron;
using Back.Data.Repository;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using Back.Servico.Email;
using Back.Servico.Hubs.Notificacoes;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

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
            services.AddScoped<IRepositorioConsulta<SincronizarItem>, RepositorioConsulta<SincronizarItem>>();

            #endregion

            #region COMANDOS

            services.AddScoped<IRepositorioComando<Configuracao>, RepositorioComando<Configuracao>>();
            services.AddScoped<IRepositorioComando<Conta>, RepositorioComando<Conta>>();
            services.AddScoped<IRepositorioComando<Sincronizar>, RepositorioComando<Sincronizar>>();
            services.AddScoped<IRepositorioComando<SincronizarItem>, RepositorioComando<SincronizarItem>>();

            #endregion


            #region SERVICOS
            services.AddScoped<NotificationHubService>();
            services.AddScoped<SincronizarCron>();
            services.AddScoped<EmailService>();
            #endregion

            return services;
        }
    }
}
