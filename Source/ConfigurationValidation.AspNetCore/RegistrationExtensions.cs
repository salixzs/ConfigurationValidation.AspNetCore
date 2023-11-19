using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ConfigurationValidation.AspNetCore;

/// <summary>
/// Register extension on approaches for configuration validation in AspNet application.
/// </summary>
public static class RegistrationExtensions
{
    /// <summary>
    /// Add an <see cref="IStartupFilter" /> to the application that invokes <see cref="IValidatableConfiguration.Validate" />
    /// on all registered configuration/settings strongly typed objects.
    /// </summary>
    /// <param name="services">The services - ASP.NET IoC container.</param>
    public static IServiceCollection AddConfigurationValidation(this IServiceCollection services) =>
        services.AddTransient<IStartupFilter, ConfigurationValidationStartupFilter>();

    /// <summary>
    /// Adds configuration validation yellow screen of death in case there are configuration validation errors found.<br/>
    /// UseWhen filters that behaviour only for front(root == "/") page.
    /// </summary>
    /// <param name="app">The ASP.NET application.</param>
    public static void UseConfigurationValidationErrorPage(this IApplicationBuilder app) =>
        app.UseWhen(context => context.Request.Path.Value.Equals("/", StringComparison.OrdinalIgnoreCase),
            appBuilder => appBuilder.UseMiddleware<ConfigurationValidationMiddleware>());

    /// <summary>
    /// Registers Configuration HealthChecking.
    /// </summary>
    /// <param name="services">ASP.Net IoC container (services).</param>
    /// <param name="isDevelopment">Should use builder.Environment.IsDevelopment() to supply boolean to avoid exposing sensitive information in production. (Default = false)</param>
    public static void AddConfigurationHealthCheck(this IServiceCollection services, bool isDevelopment = false) =>
        services.AddHealthChecks()
            .Add(new HealthCheckRegistration("Configuration", sp => new ConfigurationHealthCheck(sp.GetServices<IValidatableConfiguration>(), isDevelopment), HealthStatus.Unhealthy, null, TimeSpan.FromSeconds(3)));
}
