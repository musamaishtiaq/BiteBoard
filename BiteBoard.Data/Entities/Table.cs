using BiteBoard.Data.Abstractions;
using BiteBoard.Data.Enums;
using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiteBoard.Data.Entities
{
    [MultiTenant]
    public class Table : AuditableEntity
    {
        [Required]
        public int TableNumber { get; set; }

        public int Capacity { get; set; }

        public TableStatus Status { get; set; } = TableStatus.Available;
    }
}
