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
    public class DealItem : BaseEntity
    {
        public Guid DealId { get; set; }

        public Guid MenuItemId { get; set; }

        public int Quantity { get; set; } = 1;

        // Navigation Properties
        public virtual Deal Deal { get; set; }
        public virtual MenuItem MenuItem { get; set; }
    }
}
