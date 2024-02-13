using Back.Servico.Comandos.Board.SincronizarBoard;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Back.API.Configuracoes.Jobs
{
    public class SincronizarCron : IJob
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SincronizarCron> _logger;

        public SincronizarCron(IMediator mediator, ILogger<SincronizarCron> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation($"Cron sincronizar iniciado: {DateTime.Now}");
                var result = await _mediator.Send(new ParametroSincronizarBoard());
                _logger.LogInformation($"Cron sincronizar final: {DateTime.Now}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cron sincronizar erro: {ex.Message}");
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
