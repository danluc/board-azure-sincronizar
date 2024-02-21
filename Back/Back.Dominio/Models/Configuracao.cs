using System;

namespace Back.Dominio.Models
{
    public class Configuracao : BaseEntity
    {
        public int Dia { get; set; } = 1;
        public string HoraCron { get; set; } = "11:00";
        public string Cliente { get; set; }
    }
}
