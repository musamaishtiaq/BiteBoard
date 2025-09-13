using Finbuckle.MultiTenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiteBoard.Data.Entities
{
    [MultiTenant]
    public class MenuItemModifier
    {
        public Guid MenuItemId { get; set; }

        public Guid ModifierGroupId { get; set; }

        // Navigation Properties
        public virtual MenuItem MenuItem { get; set; }
        public virtual ModifierGroup ModifierGroup { get; set; }
    }
}
