using BiteBoard.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace BiteBoard.Api.DTOs.Requests
{
    public class DeliveryAssignmentDto
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string DriverId { get; set; }
        public DeliveryStatus Status { get; set; }
        public DateTime AssignedTime { get; set; }
        public DateTime? DeliveredTime { get; set; }
    }
}
