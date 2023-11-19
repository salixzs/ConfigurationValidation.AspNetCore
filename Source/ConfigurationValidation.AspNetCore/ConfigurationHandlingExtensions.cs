using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ConfigurationValidation.AspNetCore;

/// <summary>
/// Extensions to add configuration validation.
/// </summary>
public static class ConfigurationHandlingExtensions
{
    /// <summary>
    /// Registers a configuration instance which <typeparamref name="TConfigSection" /> will bind against, and registers as a validatable setting.
    /// Additionally registers the configuration object directly with the DI container, so can be retrieved without referencing IOptions.
    /// </summary>
    /// <typeparam name="TConfigSection">The strongly typed configuration settings - options.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection " /> to add the services to (ASP.NET IoC container).</param>
    /// <param name="configuration">The configuration section being bound.</param>
    public static IServiceCollection ConfigureValidatableSetting<TConfigSection>(this IServiceCollection services, IConfiguration configuration)
        where TConfigSection : class, IValidatableConfiguration, new()
    {
        services.Configure<TConfigSection>(configuration);
        services.AddSingleton(ctx => ctx.GetRequiredService<IOptions<TConfigSection>>().Value);
        services.AddSingleton<IValidatableConfiguration>(ctx => ctx.GetRequiredService<IOptions<TConfigSection>>().Value);
        return services;
    }

    /// <summary>
    /// Registers a configuration instance which <typeparamref name="TConfigSection" /> will bind against, and registers as a validatable setting.
    /// Additionally registers the configuration object directly with the DI container, so can be retrieved without referencing IOptions.
    /// Returns registered section as strongly typed object, so it can be used by calling party right away.
    /// </summary>
    /// <typeparam name="TConfigSection">The strongly typed configuration settings - options.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection " /> to add to (ASP.NET IoC container).</param>
    /// <param name="configuration">The configuration section being bound.</param>
    public static IServiceCollection ConfigureSetting<TConfigSection>(this IServiceCollection services, IConfiguration configuration)
        where TConfigSection : class, new()
    {
        services.Configure<TConfigSection>(configuration);
        services.AddSingleton(ctx => ctx.GetRequiredService<IOptions<TConfigSection>>().Value);
        return services;
    }
}
