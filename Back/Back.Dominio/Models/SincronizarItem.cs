using System.ComponentModel.DataAnnotations.Schema;

namespace Back.Dominio.Models
{
    public class SincronizarItem : BaseEntity
    {
        public SincronizarItem()
        {}
        public SincronizarItem(string tipo, int? origem, int? destino, string status, string erro = "")
        {
            Status = status;
            Tipo = tipo;
            Erro = erro;
            Origem = origem ?? 0;
            Destino = destino ?? 0;
        }

        public string Status { get; set; }
        public string Tipo { get; set; }
        public string Erro { get; set; }
        public int Origem { get; set; }
        public int Destino { get; set; }
        [ForeignKey("Sincronizar")]
        public int SincronizarId { get; set; }
    }
}
