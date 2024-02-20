using Back.Dominio;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Back.Servico.Hubs.Abstractions
{
    public abstract class RequiredAuthorizationHub : Hub
    {
        protected string GrupoLocal = Constantes.NOTIFICACAO_GRUPO_LOCAL;

        public override Task OnConnectedAsync()
        {
            Groups.AddToGroupAsync(Context.ConnectionId, GrupoLocal);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Groups.RemoveFromGroupAsync(GrupoLocal, Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
