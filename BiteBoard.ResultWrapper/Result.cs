namespace BiteBoard.ResultWrapper;

public class Result : IResult
{
	public bool Succeeded { get; set; }
	public string Message { get; set; }
	public Dictionary<string, List<string>> Errors { get; set; }
	public bool Failed => !Succeeded;

	public Result() { Succeeded = false; Message = string.Empty; Errors = new Dictionary<string, List<string>>(); }

	public static Result Success()
	{
		return new Result { Succeeded = true };
	}

	public static Result Success(string message)
	{
		return new Result { Succeeded = true, Message = message };
	}

	public static Result Fail()
	{
		return new Result { Succeeded = false };
	}

	public static Result Fail(string message)
	{
		return new Result { Succeeded = false, Message = message };
	}

	public static Result Fail(Dictionary<string, List<string>> errors)
	{
		return new Result { Succeeded = false, Errors = errors };
	}

	public static Result Fail(string message, Dictionary<string, List<string>> errors)
	{
		return new Result { Succeeded = false, Message = message, Errors = errors };
	}

	public Result AddError(string key, string message)
	{
		if (!Errors.TryGetValue(key, out List<string> value))
		{
			value = new List<string>();
			Errors[key] = value;
		}
		value.Add(message);
		return this;
	}

	public Result AddErrors(string key, IEnumerable<string> messages)
	{
		if (!Errors.TryGetValue(key, out List<string> value))
		{
			value = new List<string>();
			Errors[key] = value;
		}
		value.AddRange(messages);
		return this;
	}
}

public class Result<T> : Result, IResult<T>
{
	public T Data { get; set; }

	public Result() { Succeeded = false; Message = string.Empty; Errors = new Dictionary<string, List<string>>(); }

	public static new Result<T> Success()
	{
		return new Result<T> { Succeeded = true };
	}

	public static new Result<T> Success(string message)
	{
		return new Result<T> { Succeeded = true, Message = message };
	}

	public static Result<T> Success(T data)
	{
		return new Result<T> { Succeeded = true, Data = data };
	}

	public static Result<T> Success(T data, string message)
	{
		return new Result<T> { Succeeded = true, Data = data, Message = message };
	}

	public static new Result<T> Fail()
	{
		return new Result<T> { Succeeded = false };
	}

	public static new Result<T> Fail(string message)
	{
		return new Result<T> { Succeeded = false, Message = message };
	}

	public static new Result<T> Fail(Dictionary<string, List<string>> errors)
	{
		return new Result<T> { Succeeded = false, Errors = errors };
	}

	public static new Result<T> Fail(string message, Dictionary<string, List<string>> errors)
	{
		return new Result<T> { Succeeded = false, Message = message, Errors = errors };
	}
}