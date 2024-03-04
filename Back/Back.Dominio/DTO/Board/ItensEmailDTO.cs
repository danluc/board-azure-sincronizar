namespace Back.Dominio.DTO.Board
{
    public class ItensEmailDTO
    {
        public ItensEmailDTO(string tipo, int? origem, int? destino, string status)
        {
            Status = status;
            Tipo = tipo;
            Origem = origem ?? 0;
            Destino = destino ?? 0;
        }

        public string Status { get; set; }
        public string Tipo { get; set; }
        public int Origem { get; set; }
        public int Destino { get; set; }
    }
}
