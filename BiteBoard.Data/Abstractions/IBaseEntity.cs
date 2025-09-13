using System;

namespace BiteBoard.Data.Abstractions
{
    public interface IBaseEntity
    {
        public Guid Id { get; set; }
    }
}