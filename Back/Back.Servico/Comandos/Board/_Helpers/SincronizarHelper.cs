using Back.Dominio;
using Back.Dominio.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Back.Servico.Comandos.Board._Helpers
{
    public static class SincronizarHelper
    {
        #region PRIVADOS
        private static Dictionary<string, string> DicionarioMotivos()
        {
            Dictionary<string, string> Motivos = new Dictionary<string, string>()
            {
               {"Erro de desenvolvimento", "Erro de dev"},
               {"Bug Duplicado", "Bug Duplicado"},
               {"Erro de ambiente (ambiente desatualizado)", "Erro de ambiente"},
               {"Erro de merge", "Erro de merge"},
               {"Erro de negócio (cenário não considerado no refinamento)", "Erro de definição"},
               {"Não é um erro (mal entendimento do cenário)", "Não é um erro"},
               {"Não foi possível reproduzir o erro", "Não é um erro"},
            };

            return Motivos;
        }

        private static Dictionary<string, bool> DicionarioMotivosEhBug()
        {
            Dictionary<string, bool> Motivos = new Dictionary<string, bool>()
            {
               {"Erro de desenvolvimento", true},
               {"Bug Duplicado", false},
               {"Erro de ambiente (ambiente desatualizado)", false},
               {"Erro de merge", true},
               {"Erro de negócio (cenário não considerado no refinamento)", false},
               {"Não é um erro (mal entendimento do cenário)", false},
               {"Não foi possível reproduzir o erro", false},
            };

            return Motivos;
        }

        private static Dictionary<string, string> DicionarioStatusItem()
        {
            Dictionary<string, string> Status = new Dictionary<string, string>()
            {
               //HISTRIAS
               {"Backlog",              "New"},
               {"Em refinamento",       "inDesign"},
               {"Refinado",             "inDesign"},
               {"A iniciar",            "inDesign"},
               {"Em Desenvolvimento",   "inDevelopment"},
               {"Desenvolvido",         "InQA"},
               {"Em Testes",            "InQA"},
               {"Testado",              "QADone"},
               {"Em Homologação",       "InClient"},
               {"Homologado",           "clientDone"},
               {"Implantação",          "clientDone"},
               {"Em produção",          "Closed"},
               {"Em Removido",          "Removed"},

               //TASK
               {"To do",    "New"},
               {"Doing",    "Active"},
               {"Done",     "Closed"},
               {"Removido", "Blocked"},

               //BUG
               {"Promote to Production",       "Active"},
               {"Em construção",               "Active"},
               {"Validação",                   "Active"},
               {"Reaberto",                    "Active"},
               {"Closed",                      "Closed"},
               {"Waiting",                     "Active"},
               {"Develop In Progress",         "Active"},

               //SOLICITAÇÃO
               {"WIP",      "inDesign"},
               {"Blocked",  "clientDone"},
               {"Resolved", "clientDone"},
               {"Canceled", "Removed"},
            };

            return Status;
        }
        #endregion

        public static string ObterAssignedTo(object assignedTo) => assignedTo is string ? assignedTo.ToString() : ((IdentityRef)assignedTo).UniqueName;

        public static void AdicionarOperacao(JsonPatchDocument patchDocument, Operation operacao, string path, string value)
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = operacao,
                Path = path,
                Value = value
            });
        }

        public static string BuscarMotivo(string motivoPrimario, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(motivoPrimario))
                return "Não é um erro";

            var motivo = configuration.GetSection($"MotivosBug:{motivoPrimario}")?.Value;
            //var motivo = DicionarioMotivos().Where(c => c.Key.ToUpper() == motivoPrimario.ToUpper()).Select(c => c.Value).FirstOrDefault();
            if (motivo is null)
                return "Não é um erro";

            return motivo;
        }

        public static string BuscarMotivoEhBug(string motivoPrimario, IConfiguration configuration)
        {
            if (string.IsNullOrEmpty(motivoPrimario))
                return "Não";

            var resultado = Convert.ToBoolean(configuration.GetSection($"MotivosEhBug:{motivoPrimario}")?.Value);

            if (resultado)
                return "Sim";
            else
                return "Não";
        }

        public static string BuscarStatusItem(string status, string tipo, IConfiguration configuration)
        {
            var motivo = configuration.GetSection($"EstadosItens:{tipo}:{status}")?.Value;
            //var motivo = DicionarioStatusItem().Where(c => c.Key.ToUpper() == status.ToUpper()).Select(c => c.Value).FirstOrDefault();
            if (motivo is null)
                return "New";

            return motivo;
        }

        public static string RetornarTipoItem(string tipoTask, IConfiguration configuration)
        {
            var _tiposSemParente = configuration.GetSection("ItensBuscar:TiposSemParente").Get<List<string>>().Where(e => e != Constantes.TIPO_ITEM_INCIDENTE).ToList();

            if (_tiposSemParente.Contains(tipoTask))
                return Constantes.TIPO_ITEM_HISTORIA;
            else if (tipoTask == Constantes.TIPO_ITEM_INCIDENTE)
                return "Incidente";
            else
                return tipoTask;
        }
    }
}
