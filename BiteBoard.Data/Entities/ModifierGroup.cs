using BiteBoard.Data.Abstractions;
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
    public class ModifierGroup : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public int MinSelection { get; set; }

        public int MaxSelection { get; set; }

        // Navigation Properties
        public virtual ICollection<ModifierOption> ModifierOptions { get; set; }
        public virtual ICollection<MenuItemModifier> MenuItemModifiers { get; set; }
    }
}
