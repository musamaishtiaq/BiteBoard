using System;

namespace BiteBoard.Data.Abstractions
{
    public interface IAuditableEntity : IBaseEntity
    {
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}