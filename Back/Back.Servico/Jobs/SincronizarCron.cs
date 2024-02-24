using Back.Servico.Comandos.Board.SincronizarBoard;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Threading.Tasks;

namespace Back.Servico.Jobs
{
    public class SincronizarCron : IJob
    {
        private readonly IMediator _mediator;

        public SincronizarCron(IMediator mediator)
        {
            _mediator = mediator;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await _mediator.Send(new ParametroSincronizarBoard());
            }
            catch (Exception)
            { }
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
