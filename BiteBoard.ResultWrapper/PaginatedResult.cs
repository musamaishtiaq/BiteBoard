namespace BiteBoard.ResultWrapper;

public class PaginatedResult<T> : Result
{
	public T Data { get; set; }
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int TotalPages { get; set; }
	public int TotalRecords { get; set; }
	public bool HasPreviousPage => PageNumber > 1;
	public bool HasNextPage => PageNumber < TotalPages;

	public PaginatedResult(T data)
	{
		Data = data;
	}

	internal PaginatedResult(bool succeeded, string message, Dictionary<string, List<string>> errors, T data = default, int pageNumber = 1, int pageSize = 10, int totalRecords = 0)
	{
		Succeeded = succeeded;
		Message = message;
		Errors = errors;
		Data = data;
		PageNumber = pageNumber;
		PageSize = pageSize;
		TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
		TotalRecords = totalRecords;
	}

	public static PaginatedResult<T> Failure(string message, Dictionary<string, List<string>> errrors)
	{
		return new PaginatedResult<T>(false, message, errrors, default);
	}

	public static PaginatedResult<T> Success(T data, string message, int pageNumber, int pageSize, int totalRecords)
	{
		return new PaginatedResult<T>(true, message, null, data, pageNumber, pageSize, totalRecords);
	}
}