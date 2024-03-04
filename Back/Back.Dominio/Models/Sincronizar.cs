using System;
using System.Collections.Generic;

namespace Back.Dominio.Models
{
    public class Sincronizar : BaseEntity
    {
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int Status { get; set; }

        public virtual ICollection<SincronizarItem> Itens { get; set; }
    }
}
