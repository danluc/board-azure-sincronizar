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

               //SOLICITAÇÃO
               {"WIP",      "Active"},
               {"Blocked",  "Blocked"},
               {"Resolved", "Closed"},
               {"Canceled", "Blocked"},
            };

            return Status;
        }

        private static string ObterAssignedTo(object assignedTo) => assignedTo is string ? assignedTo.ToString() : ((IdentityRef)assignedTo).UniqueName;
        #endregion

        public static void AdicionarOperacao(JsonPatchDocument patchDocument, Operation operacao, string path, string value)
        {
            patchDocument.Add(new JsonPatchOperation
            {
                Operation = operacao,
                Path = path,
                Value = value
            });
        }

        //Objeto para insert ou update no azure
        public static JsonPatchDocument TratarObjeto(WorkItem item, int historiaId, Conta _conta, int areaId, Operation operacao = Operation.Add)
        {
            var tipoTask = item.Fields["System.WorkItemType"].ToString();
            var patchDocument = new JsonPatchDocument();
            string descricaoCompleta = "";

            AdicionarOperacao(patchDocument, operacao, "/fields/System.Title", $"{item.Id}: {item.Fields["System.Title"]}");
            AdicionarOperacao(patchDocument, operacao, "/fields/System.AreaId", areaId.ToString());

            //coloca a sprint so no cadastro
            if (operacao == Operation.Add)
                AdicionarOperacao(patchDocument, operacao, "/fields/System.IterationPath", _conta.Sprint.Replace("Iteration\\", ""));

            var estado = operacao == Operation.Add ? Constantes.STATUS_NOVO : BuscarStatusItem(item.Fields["System.State"].ToString());
            AdicionarOperacao(patchDocument, operacao, "/fields/System.State", estado);

            var assignedTo = operacao == Operation.Add ? _conta.NomeUsuario : ObterAssignedTo(item.Fields["System.AssignedTo"]);
            AdicionarOperacao(patchDocument, operacao, "/fields/System.AssignedTo", assignedTo);

            var descricao = item.Fields.FirstOrDefault(c => c.Key == "System.Description").Value;
            if (descricao != null)
            {
                descricaoCompleta = descricao.ToString();
                var steps = item.Fields.FirstOrDefault(c => c.Key == "Microsoft.VSTS.TCM.ReproSteps").Value;
                if (steps != null)
                    descricaoCompleta += $"<br/> {steps}";
            }

            //Se for Task ou Bug vincula a uma historia
            if ((tipoTask == Constantes.TIPO_ITEM_TASK || tipoTask == Constantes.TIPO_ITEM_BUG))
            {
                //Campo obrigatorio BUG
                if (tipoTask == Constantes.TIPO_ITEM_BUG)
                    AdicionarOperacao(patchDocument, operacao, "/fields/Custom.BUGem", "Homologação");

                //Link com a historia pai
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

            if (tipoTask == Constantes.TIPO_ITEM_SOLICITACAO)
            {
                var descricaoServiceNow = item.Fields.FirstOrDefault(c => c.Key == "Custom.DescricaoServiceNow").Value?.ToString();
                if (descricaoServiceNow != null)
                    descricaoCompleta += $"<br/> {descricaoServiceNow}";
            }

            if (tipoTask == Constantes.TIPO_ITEM_HISTORIA)
            {
                var tecnico = item.Fields.FirstOrDefault(c => c.Key == "Custom.e0b31173-d9a6-471a-afe3-7a52d4e0ff9d").Value?.ToString();
                if (tecnico != null)
                    AdicionarOperacao(patchDocument, operacao, "/fields/Custom.Detalhe", tecnico);


                var aceite = item.Fields.FirstOrDefault(c => c.Key == "Custom.88c5eab7-acc0-42bf-a522-614c59da35b0").Value?.ToString();
                if (aceite != null)
                    descricaoCompleta += $"<br/> {aceite}";
            }

            AdicionarOperacao(patchDocument, operacao, "/fields/System.Description", descricaoCompleta);

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

        public static string RetornarTipoItem(string tipoTask)
        {
            if (tipoTask == Constantes.TIPO_ITEM_ENABLER || tipoTask == Constantes.TIPO_ITEM_DEBITO || tipoTask == Constantes.TIPO_ITEM_SOLICITACAO)
                return Constantes.TIPO_ITEM_HISTORIA;
            else
                return tipoTask;
        }
    }
}
