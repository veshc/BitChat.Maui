namespace BitChat.Maui;

/// <summary>
/// Helper class for accessing services from dependency injection container
/// </summary>
public static class ServiceHelper
{
    /// <summary>
    /// Gets a service from the dependency injection container
    /// </summary>
    /// <typeparam name="T">The service type to retrieve</typeparam>
    /// <returns>The service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not registered</exception>
    public static T GetService<T>() where T : class
    {
        var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;
        
        if (serviceProvider == null)
            throw new InvalidOperationException("Service provider is not available");

        var service = serviceProvider.GetService<T>();
        
        if (service == null)
            throw new InvalidOperationException($"Service of type {typeof(T).Name} is not registered");

        return service;
    }

    /// <summary>
    /// Gets a required service from the dependency injection container
    /// </summary>
    /// <typeparam name="T">The service type to retrieve</typeparam>
    /// <returns>The service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not registered</exception>
    public static T GetRequiredService<T>() where T : class
    {
        var serviceProvider = Application.Current?.Handler?.MauiContext?.Services;
        
        if (serviceProvider == null)
            throw new InvalidOperationException("Service provider is not available");

        var service = serviceProvider.GetRequiredService<T>();
        return service;
    }
}