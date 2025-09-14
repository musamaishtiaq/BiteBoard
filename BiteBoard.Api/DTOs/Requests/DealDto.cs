using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiteBoard.Api.DTOs.Requests
{
    public class DealDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public List<DealItemDto> DealItems { get; set; }
    }
}
