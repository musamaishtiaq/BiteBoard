using BiteBoard.Data.Entities;
using BiteBoard.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace BiteBoard.Api.DTOs.Requests
{
    public class OrderDto
    {
        public string Id { get; set; }
        public OrderType OrderType { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DeliveryFee { get; set; }
        public string TableId { get; set; }
        public string CustomerName { get; set; }
        public string Notes { get; set; }

        public List<OrderItemDto> OrderItems { get; set; }
        public TableDto Table { get; set; }
        public DeliveryAddressDto DeliveryAddress { get; set; }
        public DeliveryAssignmentDto DeliveryAssignment { get; set; }
    }
}
