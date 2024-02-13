namespace Back.Dominio.Models
{
    public class Conta : BaseEntity
    {
        public string Token { get; set; }
        public string NomeUsuario { get; set; }
        public string UrlCorporacao { get; set; }
        public string NomeProjeto { get; set; }
        public string TimeId { get; set; }
        public string ProjetoId { get; set; }
        public string AreaPath { get; set; }
        public bool Principal { get; set; }
    }
}
