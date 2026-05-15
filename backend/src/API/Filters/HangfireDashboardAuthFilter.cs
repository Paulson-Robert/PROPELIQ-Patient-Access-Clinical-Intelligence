using Hangfire.Dashboard;

namespace API.Filters;

/// <summary>
/// Fail-closed Hangfire dashboard authorization filter.
/// Requires the user to be authenticated and hold the Admin role.
/// Returns false (deny → 401) for every unauthenticated or non-Admin request.
/// </summary>
public sealed class HangfireDashboardAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        return httpContext.User.IsInRole("Admin");
    }
}
