using System.ComponentModel.DataAnnotations;

namespace BiteBoard.Api.DTOs.Requests
{
    public class ModifierGroupDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public int MinSelection { get; set; }
        public int MaxSelection { get; set; }

        public List<ModifierOptionDto> ModifierOptions { get; set; }
    }
}
