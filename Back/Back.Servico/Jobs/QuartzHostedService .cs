using Back.Dominio;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Back.Servico.Jobs
{
    public class QuartzHostedService
    {
        /*private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IScheduler _scheduler;

        public QuartzHostedService(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, IScheduler scheduler)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _scheduler = scheduler;
        }
        public async Task AtualizarHoraJob(CancellationToken cancellationToken, string horario)
        {
            

     
            var horaCron = horario.Split(":");

            await _scheduler.UnscheduleJob(new TriggerKey(Constantes.JOB_UM_TRIGGER, Constantes.JOB_GRUPO_UM));

            var sincronizarJob = JobBuilder.Create<SincronizarCron>().WithIdentity(Constantes.JOB_UM, Constantes.JOB_GRUPO_UM).Build();


            var sincronizarJobTrigger = TriggerBuilder.Create()
                     .WithIdentity(Constantes.JOB_UM_TRIGGER, Constantes.JOB_GRUPO_UM)
                     .StartNow()
                     .WithDailyTimeIntervalSchedule(c => c.WithIntervalInHours(24)//intervalo de hrs
                                                         .OnEveryDay()//todos os dias
                                                         .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(Convert.ToInt32(horaCron[0]), Convert.ToInt32(horaCron[1])))//hr q começa
                                                  ).Build();

            await _scheduler.ScheduleJob(sincronizarJob, sincronizarJobTrigger);

            // Agendar o novo trigger para o job
            //await Scheduler.RescheduleJob(new TriggerKey(Constantes.JOB_UM_TRIGGER, Constantes.JOB_GRUPO_UM), sincronizarJobTrigger);
        }*/
    }
}
