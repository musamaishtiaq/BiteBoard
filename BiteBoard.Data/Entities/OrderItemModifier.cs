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
    public class OrderItemModifier : BaseEntity
    {
        public Guid OrderItemId { get; set; }

        public Guid ModifierOptionId { get; set; }

        // Navigation Properties
        public virtual OrderItem OrderItem { get; set; }
        public virtual ModifierOption ModifierOption { get; set; }
    }
}
