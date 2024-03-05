using System;

namespace Back.Dominio.DTO.Areas
{
    public class AreaDTO
    {
        public AreaDTO()
        {

        }

        public AreaDTO(Guid id, string name, string path)
        {
            Id = id;
            Name = name;
            Path = path;
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }
}
