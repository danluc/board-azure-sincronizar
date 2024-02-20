using APP.Configuracoes.Jobs;
using Back.Data.Context;
using Back.Dominio.Interfaces;
using Back.Dominio.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Quartz;
using Quartz.Impl;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Daniel - Board",
                        Version = "v1",
                        Description = "Daniel - Board",
                        Contact = new Contact
                        {
                            Name = "Daniel Lucas",
                            Url = "https://github.com/danluc"
                        }
                    });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                {
                    Description = "JWT Authorization header \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new string[] { } }
                });
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

                var sincronizarJob = JobBuilder.Create<SincronizarCron>().WithIdentity("SincronizarJob", "group1").Build();

                var sincronizarJobTrigger = TriggerBuilder.Create()
                    .WithIdentity("SincronizarJobTrigger", "group1")
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
