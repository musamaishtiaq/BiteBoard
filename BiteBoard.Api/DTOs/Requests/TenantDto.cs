using BiteBoard.Data.Enums;
using System;

namespace BiteBoard.API.DTOs.Requests
{
    public class TenantDto
    {
        public string Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public DateTime ValidTill { get; set; }
        public bool IsEnabled { get; set; } = true;
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
