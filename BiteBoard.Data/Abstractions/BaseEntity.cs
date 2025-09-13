using System;

namespace BiteBoard.Data.Abstractions
{
    public abstract class BaseEntity : IBaseEntity
    {
        public Guid Id { get; set; }
    }
}