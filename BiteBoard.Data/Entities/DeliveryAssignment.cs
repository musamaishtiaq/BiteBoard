using BiteBoard.Data.Abstractions;
using BiteBoard.Data.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BiteBoard.Data.Constants.Permissions;

namespace BiteBoard.Data.Entities
{
    public class DeliveryAssignment : BaseEntity
    {
        public Guid OrderId { get; set; }

        public Guid DriverId { get; set; }

        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        public DateTime AssignedTime { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveredTime { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; }
    }
}
