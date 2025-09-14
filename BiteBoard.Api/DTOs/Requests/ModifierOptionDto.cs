using System.ComponentModel.DataAnnotations;

namespace BiteBoard.Api.DTOs.Requests
{
    public class ModifierOptionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal PriceAdjustment { get; set; }
    }
}
