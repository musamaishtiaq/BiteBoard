namespace BiteBoard.ResultWrapper;

public interface IResult
{
	bool Succeeded { get; set; }
	string Message { get; set; }
	Dictionary<string, List<string>> Errors { get; set; }
}

public interface IResult<out T> : IResult
{
	T Data { get; }
}