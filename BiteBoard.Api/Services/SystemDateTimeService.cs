using BiteBoard.Data.Interfaces;

namespace BiteBoard.API.Services;

public class SystemDateTimeService : IDateTimeService
{
    public DateTime NowUtc => DateTime.UtcNow;
}