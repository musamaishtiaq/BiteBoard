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
    public class ModifierOption : AuditableEntity
    {
        public Guid ModifierGroupId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAdjustment { get; set; }

        // Navigation Properties
        public virtual ModifierGroup ModifierGroup { get; set; }
        public virtual ICollection<OrderItemModifier> OrderItemModifiers { get; set; }
    }
}
