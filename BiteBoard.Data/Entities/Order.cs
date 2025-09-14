using BiteBoard.Data.Abstractions;
using BiteBoard.Data.Enums;
using Finbuckle.MultiTenant;
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
    [MultiTenant]
    public class Order : AuditableEntity
    {
        public OrderType OrderType { get; set; }

        public OrderStatus OrderStatus { get; set; }

        public DeliveryStatus? DeliveryStatus { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeliveryFee { get; set; }

        public Guid? TableId { get; set; }

        [MaxLength(50)]
        public string CustomerName { get; set; }

        [MaxLength(250)]
        public string Notes { get; set; }

        // Navigation Properties
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual Table Table { get; set; }
        public virtual DeliveryAddress DeliveryAddress { get; set; }
        public virtual DeliveryAssignment DeliveryAssignment { get; set; }
    }
}
