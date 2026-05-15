using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using UniFlow.API.Contracts;

namespace UniFlow.API.Configuration;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddUniFlowRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
            ?? new RateLimitingOptions();

        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));

        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiterOptions.OnRejected = OnRejectedAsync;

            limiterOptions.AddPolicy(RateLimitPolicies.Ai, context =>
                CreateFixedWindowPartition(context, options.Ai));

            limiterOptions.AddPolicy(RateLimitPolicies.Upload, context =>
                CreateFixedWindowPartition(context, options.Upload));
        });

        return services;
    }

    private static RateLimitPartition<string> CreateFixedWindowPartition(
        HttpContext httpContext,
        RateLimitPolicyOptions policyOptions)
    {
        var partitionKey = ResolvePartitionKey(httpContext);

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = policyOptions.PermitLimit,
            Window = TimeSpan.FromSeconds(policyOptions.WindowSeconds),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
        });
    }

    private static string ResolvePartitionKey(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

        var ip = httpContext.Connection.RemoteIpAddress?.ToString();
        return $"ip:{ip ?? "unknown"}";
    }

    private static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var seconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
            context.HttpContext.Response.Headers.RetryAfter =
                seconds.ToString(CultureInfo.InvariantCulture);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        await context.HttpContext.Response
            .WriteAsJsonAsync(RateLimitResponse.Exceeded(), cancellationToken)
            .ConfigureAwait(false);
    }
}
