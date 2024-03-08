namespace Back.Dominio.Models
{
    public class Configuracao : BaseEntity
    {
        public int Dia { get; set; }
        public string HoraCron { get; set; }
    }
}
