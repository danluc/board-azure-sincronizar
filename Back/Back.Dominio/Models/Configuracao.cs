using System;

namespace Back.Dominio.Models
{
    public class Configuracao : BaseEntity
    {
        public int Dia { get; set; } = 1;
        public string HoraCron { get; set; } = "11:00";
        public string Cliente { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string SMTP { get; set; }
        public int Porta { get; set; }


        public static bool VerificarSeExisteEmail(Configuracao dados)
        {
            return !string.IsNullOrEmpty(dados.Email) &&
                    !string.IsNullOrEmpty(dados.Senha) &&
                    !string.IsNullOrEmpty(dados.SMTP) &&
                    dados.Porta > 0;
        }
    }
}
