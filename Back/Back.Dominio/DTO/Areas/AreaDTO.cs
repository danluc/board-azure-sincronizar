using System;

namespace Back.Dominio.DTO.Areas
{
    public class AreaDTO
    {
        public AreaDTO()
        { }

        public AreaDTO(int id, Guid identificador, string name, string path)
        {
            Id = id;
            Identificador = identificador;
            Name = name;
            Path = path;
        }

        public int Id { get; set; }
        public Guid Identificador { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
