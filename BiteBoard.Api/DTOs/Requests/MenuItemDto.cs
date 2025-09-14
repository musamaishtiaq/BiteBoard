using System.ComponentModel.DataAnnotations;

namespace BiteBoard.Api.DTOs.Requests
{
    public class MenuItemDto
    {
        public string Id { get; set; }
        public string CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsAvailable { get; set; }

        public List<MenuItemModifierDto> MenuItemModifiers { get; set; }
    }
}
