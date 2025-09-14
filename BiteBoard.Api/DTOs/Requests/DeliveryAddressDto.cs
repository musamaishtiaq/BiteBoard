using System.ComponentModel.DataAnnotations;

namespace BiteBoard.Api.DTOs.Requests
{
    public class DeliveryAddressDto
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }
        public string Instructions { get; set; }
    }
}
