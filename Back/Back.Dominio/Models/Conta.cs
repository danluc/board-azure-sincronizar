namespace Back.Dominio.Models
{
    public class Conta : BaseEntity
    {
        public string EmailDe { get; set; }
        public string EmailPara { get; set; }
        public long AreaId { get; set; }
        public string AreaPath { get; set; }
        public string Sprint { get; set; }
        public long SprintId { get; set; }
        public string Cliente { get; set; }
    }
}