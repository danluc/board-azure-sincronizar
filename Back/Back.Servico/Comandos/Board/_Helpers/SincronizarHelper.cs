using Back.Dominio;
using Back.Dominio.Models;
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
               //{"Backlog",       "New"},
               {"Em construção","Active"},
               {"Validação",    "Active"},
               {"Reaberto",     "Active"},
               {"Closed",       "Closed"},
               //{"Removido",     "Blocked"},
            };

            return Status;
        }

        //Objeto para insert ou update no azure
        public static JsonPatchDocument TratarObjeto(WorkItem item, int historiaId, Conta _conta, int areaId, Operation operacao = Operation.Add)
        {
            var tipoTask = item.Fields["System.WorkItemType"].ToString();
            JsonPatchDocument patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.Title",
                    Value = $"{item.Id}: {item.Fields["System.Title"]}",
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.AreaId",
                    Value = $"{areaId}"
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.IterationPath",
                    Value = $"{_conta.Sprint.Replace("Iteration\\", "")}"
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.State",
                    Value = operacao == Operation.Add ? Constantes.STATUS_NOVO : BuscarStatusItem(item.Fields["System.State"].ToString()),
                },
                new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.AssignedTo",
                    Value = operacao == Operation.Add ? _conta.NomeUsuario : (item.Fields["System.AssignedTo"] is string) ? item.Fields["System.AssignedTo"] : ((IdentityRef)item.Fields["System.AssignedTo"]).UniqueName,
                }
            };

            //Verifica se tem descrição
            var descricao = item.Fields.FirstOrDefault(c => c.Key == "System.Description").Value;
            if (descricao != null)
            {
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.Description",
                    Value = $"{item.Fields["System.Description"]}",
                });
            }

            //Verifica se tem step
            var steps = item.Fields.FirstOrDefault(c => c.Key == "Microsoft.VSTS.TCM.ReproSteps").Value;
            if (steps != null)
            {
                descricao = (descricao == null) ? steps : $"{descricao} <br/> {steps}";
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/System.Description",
                    Value = $"{descricao}",
                });
            }

            //Campo obrigatorio BUG
            if (tipoTask == Constantes.TIPO_ITEM_BUG)
            {
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = operacao,
                    Path = "/fields/Custom.BUGem",
                    Value = $"Homologação",
                });
            }
            
            //Se for Task ou Bug vincula a uma historia
            if ((tipoTask == Constantes.TIPO_ITEM_TASK || tipoTask == Constantes.TIPO_ITEM_BUG))
            {
                patchDocument.Add(new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"{_conta.UrlCorporacao}/{_conta.ProjetoNome}/_apis/wit/workItems/{historiaId}",
                    }
                });
            }

            return patchDocument;
        }

        public static string BuscarMotivo(string motivoPrimario)
        {
            var motivo = DicionarioMotivos().Where(c => c.Key.ToUpper() == motivoPrimario.ToUpper()).Select(c => c.Value).FirstOrDefault();
            if (motivo is null)
                return "Não é um erro";

            return motivo;
        }

        public static string BuscarStatusItem(string status)
        {
            var motivo = DicionarioStatusItem().Where(c => c.Key.ToUpper() == status.ToUpper()).Select(c => c.Value).FirstOrDefault();
            if (motivo is null)
                return "New";

            return motivo;
        }
    }
}
