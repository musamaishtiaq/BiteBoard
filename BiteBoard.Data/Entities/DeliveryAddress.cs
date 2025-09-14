using BiteBoard.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiteBoard.Data.Entities
{
    public class DeliveryAddress : BaseEntity
    {
        public Guid OrderId { get; set; }

        [MaxLength(15)]
        public string CustomerPhone { get; set; }

        [MaxLength(150)]
        public string CustomerAddress { get; set; }

        [MaxLength(250)]
        public string Instructions { get; set; }

        // Navigation Properties
        public virtual Order Order { get; set; }
    }
}
