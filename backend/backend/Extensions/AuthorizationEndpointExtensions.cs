using backend.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace backend.Extensions;

public static class AuthorizationEndpointExtensions
{
    public static TBuilder RequireAdmin<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(AuthorizationPolicies.AdminOnly);
    }
}