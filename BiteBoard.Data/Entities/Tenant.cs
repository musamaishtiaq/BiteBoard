using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiteBoard.Data.Entities
{
    public class Tenant : ITenantInfo
    {
        public string Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Identifier { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(250)]
        public string ConnectionString { get; set; }

        public DateTime ValidTill { get; set; }

        public bool IsEnabled { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
