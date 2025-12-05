using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace FBC.Devices.API.MediatR;

public static class MediatorExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        //services.AddSingleton<IMediator, Mediator>();
        services.AddScoped<IMediator, FBCMediator>();
        var allAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        var handlerTypes = allAssemblies.SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                             i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
                .Select(i => new { HandlerType = t, InterfaceType = i }));
        foreach (var handler in handlerTypes)
        {
            //Tüm örneklerde transient yazılmış. Ben neden scoped yaptım? 
            //services.AddTransient(handler.InterfaceType, handler.HandlerType);
            services.AddScoped(handler.InterfaceType, handler.HandlerType);
        }
        // Register IEndpoint implementations
        //var serviceDescriptors = allAssemblies.SelectMany(assembly => assembly.DefinedTypes.Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint))).Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type)).ToArray());
        //services.TryAddEnumerable(serviceDescriptors);

        var endpointTypes = allAssemblies
            .SelectMany(a => a.DefinedTypes)
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);

        foreach (var endpointType in endpointTypes)
        {
            services.AddTransient(endpointType);                    // Kendi tipiyle (opsiyonel ama iyi)
            services.AddTransient(typeof(IEndpoint), endpointType); // IEnumerable<IEndpoint> için
        }

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<SwaggerGenOptions>>(new SafeChainedSchemaIdConfigurator()));
        return services;
    }

    public static IApplicationBuilder UseMediatorEndpoints(this IApplicationBuilder app)
    {
        var endpoints = app.ApplicationServices.GetServices<IEndpoint>();
        //var routeBuilder = app.ApplicationServices.GetRequiredService<IEndpointRouteBuilder>();
        var routeBuilder = (IEndpointRouteBuilder)app;
        foreach (var endpoint in endpoints)
        {
            endpoint.AddRoutes(routeBuilder);
        }
        return app;
    }
}
internal sealed class SafeChainedSchemaIdConfigurator : IPostConfigureOptions<SwaggerGenOptions>
{
    public void PostConfigure(string name, SwaggerGenOptions options)
    {
        var existingSelector = options.SchemaGeneratorOptions.SchemaIdSelector;

        options.SchemaGeneratorOptions.SchemaIdSelector = type =>
        {
            //// 1. Kullanıcının kuralı varsa önce onu dene
            //var userResult = existingSelector?.Invoke(type);

            //// 2. Kullanıcı null veya boş string döndüyse → bizim fallback devreye girsin
            //if (!string.IsNullOrEmpty(userResult))
            //    return userResult;

            // 3. Fallback: CreateDevice_Command gibi
            return type.DeclaringType != null
                ? $"{type.DeclaringType.Name}_{type.Name}"
                : type.Name;
        };
    }
}