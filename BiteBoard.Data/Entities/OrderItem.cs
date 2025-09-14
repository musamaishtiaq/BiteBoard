using BiteBoard.Data.Abstractions;
using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiteBoard.Data.Entities
{
    [MultiTenant]
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }

        public Guid MenuItemId { get; set; }

        public Guid? DealId { get; set; } // Nullable for regular items

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [MaxLength(500)]
        public string SpecialInstructions { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; }
        public virtual MenuItem MenuItem { get; set; }
        public virtual Deal Deal { get; set; } // Nullable for regular items
        public virtual ICollection<OrderItemModifier> OrderItemModifiers { get; set; }
    }
}
