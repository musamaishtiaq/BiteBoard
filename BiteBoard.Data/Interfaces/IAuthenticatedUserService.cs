using System;

namespace BiteBoard.Data.Interfaces
{
	public interface IAuthenticatedUserService
	{
		public Guid UserId { get; }
		public string Username { get; }
	}
}