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

        public async Task Notificar(string grupoLocal)
        {
            await _notificationHubContext.Clients
                .Group(grupoLocal.ToString())
                .SendAsync(Constantes.NOTIFICACAO_PRONTA);
        }
    }
}
