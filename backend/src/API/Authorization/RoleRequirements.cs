using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace API.Authorization;

public static class RoleRequirements
{
    public const string PatientPolicy = "PatientOnly";
    public const string StaffPolicy = "StaffOnly";
    public const string AdminPolicy = "AdminOnly";

    public static IServiceCollection AddRoleAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(PatientPolicy, policy =>
                policy.RequireAssertion(context =>
                    HasSingleRole(context.User, out var role) &&
                    (HasRole(role, "Patient") || HasRole(role, "Admin"))));

            options.AddPolicy(StaffPolicy, policy =>
                policy.RequireAssertion(context =>
                    HasSingleRole(context.User, out var role) &&
                    (HasRole(role, "Staff") || HasRole(role, "Admin"))));

            options.AddPolicy(AdminPolicy, policy =>
                policy.RequireAssertion(context =>
                    HasSingleRole(context.User, out var role) && HasRole(role, "Admin")));
        });

        return services;
    }

    private static bool HasSingleRole(ClaimsPrincipal principal, out string? role)
    {
        var roles = principal.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        role = roles.Length == 1 ? roles[0] : null;
        return role is not null;
    }

    private static bool HasRole(string? actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
}
