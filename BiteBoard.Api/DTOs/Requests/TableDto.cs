using BiteBoard.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace BiteBoard.Api.DTOs.Requests
{
    public class TableDto
    {
        public string Id { get; set; }
        public int TableNumber { get; set; }
        public int Capacity { get; set; }
        public TableStatus Status { get; set; }
    }
}
