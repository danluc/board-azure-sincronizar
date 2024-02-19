using Back.Dominio.Enum;
using System;

namespace Back.Dominio.Models
{
    public class Sincronizar : BaseEntity
    {
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int Status { get; set; }
    }
}
