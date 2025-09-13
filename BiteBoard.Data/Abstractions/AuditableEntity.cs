using System;

namespace BiteBoard.Data.Abstractions
{
    public abstract class AuditableEntity : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}