using Back.Dominio.Interfaces;

namespace Back.Dominio.Models
{
    public class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }
    }
}
