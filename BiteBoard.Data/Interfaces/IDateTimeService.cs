using System;

namespace BiteBoard.Data.Interfaces;

public interface IDateTimeService
{
    public DateTime NowUtc { get; }
}