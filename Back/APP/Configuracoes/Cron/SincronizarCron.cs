using Back.Dominio;
using Back.Servico.Comandos.Board.SincronizarBoard;
using Back.Servico.Hubs.Notificacoes;
using ElectronNET.API;
using ElectronNET.API.Entities;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace APP.Configuracoes.Cron
{
    public class SincronizarCron : IJob
    {
        private readonly IMediator _mediator;
        private readonly NotificationHubService _notificationHubService;

        public SincronizarCron(IMediator mediator, NotificationHubService notificationHubService)
        {
            _mediator = mediator;
            _notificationHubService = notificationHubService;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                Electron.Notification.Show(new NotificationOptions("Sicronização Board", "Iniciada!"));
               
                await _mediator.Send(new ParametroSincronizarBoard());
                
                Electron.Notification.Show(new NotificationOptions("Sicronização Board", "Finalizada!"));
            }
            catch (Exception)
            {
                await _notificationHubService.SincronizarFim();
            }
        }
    }

    public class SingletonJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public SingletonJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
        }

        public void ReturnJob(IJob job)
        {
            var disposable = job as IDisposable;
            disposable?.Dispose();
        }
    }
}
