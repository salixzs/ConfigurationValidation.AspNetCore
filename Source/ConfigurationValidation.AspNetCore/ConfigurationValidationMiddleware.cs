using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace ConfigurationValidation.AspNetCore;

/// <summary>
/// Middleware to display configuration validation results as error page.
/// </summary>
public class ConfigurationValidationMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Middleware to display configuration validation results as error page.
    /// </summary>
    public ConfigurationValidationMiddleware(RequestDelegate next) =>
        _next = next ?? throw new ArgumentNullException(nameof(next));

    /// <summary>
    /// Overridden method which gets invoked by HTTP middleware stack.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="validatableConfigurations">Validatable configuration itens collection.</param>
    /// <param name="logger">Logger instance.</param>
    /// <exception cref="ArgumentNullException">HTTP Context does not exist (never happens).</exception>
#pragma warning disable RCS1046 // Suffix Async is not expected by ASP.NET Core implementation
    public async Task Invoke(HttpContext httpContext, IEnumerable<IValidatableConfiguration> validatableConfigurations, ILogger<ConfigurationValidationMiddleware> logger)
#pragma warning restore RCS1046
    {
        logger.LogDebug("Validating configuration of {ValidationCount} objects.", validatableConfigurations.Count());
        var failures = new List<ConfigurationValidationItem>();
        foreach (var validatableObject in validatableConfigurations)
        {
            failures.AddRange(validatableObject.Validate());
        }

        if (failures.Count == 0)
        {
            logger.LogDebug("All configurations are valid.");
            await _next(httpContext);
            return;
        }

        // Put configuration validation failures in log.
        logger.LogError("Found {FailureCount} problems in configuration.", failures.Count);
        foreach (var failure in failures)
        {
            logger.LogError("Configuration section {Section}, item {Item}: {Message}", failure.ConfigurationSection, failure.ConfigurationItem, failure.ValidationMessage);
        }

        // Now getting page template and pushing it to response
        string errorPage = PageHtml.config_errors;
        var errorTable = new StringBuilder();
        foreach (var failure in failures)
        {
            errorTable.Append($"<tr><td>{failure.ConfigurationSection}</td><td>{failure.ConfigurationItem}</td><td>{failure.ValidationMessage}</td></tr>");
        }

        errorPage = errorPage.Replace("{Validations}", errorTable.ToString());
        var response = httpContext.Response;
        if (response.HasStarted)
        {
            throw new InvalidOperationException("The response cannot be cleared, it has already started sending.");
        }

        // Clear() content.
        response.StatusCode = 200;
        response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = null;
        response.Headers.Clear();
        if (response.Body.CanSeek)
        {
            response.Body.SetLength(0L);
        }

        response.ContentType = "text/html";
        response.StatusCode = 500;
        response.Headers[HeaderNames.CacheControl] = "no-cache";
        response.Headers[HeaderNames.Pragma] = "no-cache";
        response.Headers[HeaderNames.Expires] = "-1";
        response.Headers.Remove(HeaderNames.ETag);
        await response.WriteAsync(errorPage).ConfigureAwait(false);
    }
}
