using APP.Configuracoes.Cron;
using Back.Data.Context;
using Back.Dominio;
using Back.Dominio.Enum;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Quartz;
using Quartz.Impl;
using Serilog;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace APP.Configuracoes
{
    public static class ServicesConfiguracao
    {
        //private static readonly IServiceProvider _serviceProvider;

        public static void AddVssConnections(this IServiceCollection services, string baseUrl1, string token1, string baseUrl2, string token2)
        {
            // Registrando a primeira instância de VssConnection
            services.AddSingleton<VssConnection>(new VssConnection(new Uri(baseUrl1), new VssBasicCredential(string.Empty, token1)));

            // Registrando a segunda instância de VssConnection
            services.AddSingleton<VssConnection>(new VssConnection(new Uri(baseUrl2), new VssBasicCredential(string.Empty, token2)));
        }

        public static IServiceCollection AddServicosConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BancoDBContext>();

            var assembly = AppDomain.CurrentDomain.Load("Back.Servico");
            services.AddMediatR(assembly);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SINCRONIZAR BOARD", Version = "v1" });
            });

            string basedir = AppDomain.CurrentDomain.BaseDirectory;

            //Log
            Log.Logger = new LoggerConfiguration()
            .WriteTo.File($"{basedir}/Logs/log-{DateTime.Now.ToString("yyyy-MM-dd")}.log")
            .CreateLogger();

            return services;
        }

        public static async Task AddServicosJob(this IServiceProvider services)
        {
            using (IServiceScope scope = services.CreateScope())
            {
                var repoConsultaConfiguracao = scope.ServiceProvider.GetRequiredService<IRepositorioConsulta<Configuracao>>();
                var repoConsultaSincronizar = scope.ServiceProvider.GetRequiredService<IRepositorioConsulta<Sincronizar>>();
                var repoComandoSincronizar = scope.ServiceProvider.GetRequiredService<IRepositorioComando<Sincronizar>>();
                var repoComandoSincronizarItens = scope.ServiceProvider.GetRequiredService<IRepositorioComando<SincronizarItem>>();

                var processando = await repoConsultaSincronizar.FindBy(e => e.Status == (int)EStatusSincronizar.PROCESSANDO);
                foreach (var item in processando)
                {
                    item.DataFim = DateTime.Now;
                    item.Status = (int)EStatusSincronizar.ERRO;
                    repoComandoSincronizar.Update(item);
                    await repoComandoSincronizar.SaveChangesAsync();
                }

                try
                {
                    var ultimo = await repoConsultaSincronizar.Query(e => e.DataInicio.Date <= DateTime.Now.Date.AddDays(-5)).OrderByDescending(c => c.Id).Select(c => c.Id).FirstOrDefaultAsync();
                    if(ultimo > 0)
                    {
                        repoComandoSincronizarItens.DeleteRange(e => e.SincronizarId == ultimo);
                        await repoComandoSincronizarItens.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {}

                var config = await repoConsultaConfiguracao.Query().FirstOrDefaultAsync();
                var horaCron = config.HoraCron.Split(":");

                var props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };

                var factory = new StdSchedulerFactory(props);
                var sched = await factory.GetScheduler();

                sched.JobFactory = new SingletonJobFactory(services);

                await sched.Start();

                var sincronizarJob = JobBuilder.Create<SincronizarCron>().WithIdentity(Constantes.JOB_UM, Constantes.JOB_GRUPO_UM).Build();

                var sincronizarJobTrigger = TriggerBuilder.Create()
                    .WithIdentity(Constantes.JOB_UM_TRIGGER, Constantes.JOB_GRUPO_UM)
                    .StartNow()
                    .WithDailyTimeIntervalSchedule(c => c.WithIntervalInHours(24)//intervalo de hrs
                                                        .OnEveryDay()//todos os dias
                                                        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(Convert.ToInt32(horaCron[0]), Convert.ToInt32(horaCron[1])))//hr q começa
                                                 ).Build();

                await sched.ScheduleJob(sincronizarJob, sincronizarJobTrigger);
            }
        }
    }
}
