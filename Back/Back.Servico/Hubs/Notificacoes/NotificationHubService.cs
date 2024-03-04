using Back.Dominio;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Back.Servico.Hubs.Notificacoes
{
    public class NotificationHubService
    {
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public NotificationHubService(IHubContext<NotificationHub> notificationHubContext)
        {
            _notificationHubContext = notificationHubContext;
        }

        public async Task NotificarInicio() => await _notificationHubContext.Clients.All.SendAsync(Constantes.NOTIFICACAO_INICIO);
        public async Task SincronizarInicio() => await _notificationHubContext.Clients.All.SendAsync(Constantes.NOTIFICACAO_SYNC_INICIO);
        public async Task SincronizarFim() => await _notificationHubContext.Clients.All.SendAsync(Constantes.NOTIFICACAO_SYNC_FIM);
    }
}
