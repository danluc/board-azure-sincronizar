using System.Collections.Generic;

namespace Back.Dominio.DTO.Areas
{
    public class ListaSprintsDTO
    {
        public AreaDTO Time { get; set; }
        public List<AreaDTO> Sprints { get; set; }
    }
}
