using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Back.Dominio.DTO.Board
{
    public class ItensSicronizarDTO
    {
        public WorkItem Historia { get; set; }
        public List<WorkItem> Itens { get; set; }
    }
}
