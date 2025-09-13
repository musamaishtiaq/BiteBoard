using BiteBoard.Data.Enums;
using Hangfire.Dashboard;

namespace BiteBoard.API.Filters
{
	public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context)
		{
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity.IsAuthenticated && httpContext.User.IsInRole(Roles.SuperAdmin.ToString());
        }
	}
}