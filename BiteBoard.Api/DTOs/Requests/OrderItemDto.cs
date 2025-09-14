using BiteBoard.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace BiteBoard.Api.DTOs.Requests
{
    public class OrderItemDto
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string MenuItemId { get; set; }
        public string DealId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string SpecialInstructions { get; set; }

        public virtual OrderDto Order { get; set; }
        public virtual MenuItemDto MenuItem { get; set; }
        public virtual DealDto Deal { get; set; }
        public List<OrderItemModifierDto> Modifiers { get; set; }
    }
}
