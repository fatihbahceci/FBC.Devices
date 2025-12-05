using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace FBC.Devices.API.MediatR;

public interface IRequestBase;
public interface IRequest<out TResponse> : IRequestBase;
public interface IRequest : IRequestBase;

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken token = default);
}
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    Task Handle(TRequest request, CancellationToken token = default);
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    Task Send(IRequest request, CancellationToken cancellationToken = default);
}

public class Mediator : IMediator
{
    // small marker to represent void in generic pipeline
    private class VoidMarker { public static readonly VoidMarker Instance = new(); }

    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, object> _handlerCache = new();
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    //public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    //{
    //    var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
    //    dynamic handler = _serviceProvider.GetRequiredService(handlerType);
    //    return await handler.Handle((dynamic)request, cancellationToken);
    //}
    //public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    //{
    //    var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
    //    dynamic handler = _serviceProvider.GetRequiredService(handlerType);
    //    await handler.Handle((dynamic)request, cancellationToken);
    //}

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));


        var key = GetCacheKey(request.GetType(), typeof(TResponse));
        var invoker = (Func<IServiceProvider, object, CancellationToken, Task<TResponse>>)_handlerCache.GetOrAdd(key, _ => BuildRequestInvoker<TResponse>(request.GetType()));
        return invoker(_serviceProvider, request, cancellationToken);
    }


    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));


        var key = GetCacheKey(request.GetType(), typeof(VoidMarker));
        var invoker = (Func<IServiceProvider, object, CancellationToken, Task>)_handlerCache.GetOrAdd(key, _ => BuildVoidRequestInvoker(request.GetType()));
        return invoker(_serviceProvider, request, cancellationToken);
    }

    private static string GetCacheKey(Type requestType, Type responseType) => requestType.FullName + "=>" + responseType.FullName;

    private static Func<IServiceProvider, object, CancellationToken, Task<TResponse>> BuildRequestInvoker<TResponse>(Type requestType)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var requestParam = Expression.Parameter(typeof(object), "request");
        var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");
        var getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions)
            .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService), BindingFlags.Static | BindingFlags.Public)
            .MakeGenericMethod(handlerType);

        var getServiceCall = Expression.Call(
            null, // static method
            getRequiredServiceMethod,
            serviceProviderParam);
        var handlerVar = Expression.Variable(handlerType, "handler");
        var assignHandler = Expression.Assign(handlerVar, getServiceCall);
        var handleMethod = handlerType.GetMethod("Handle");
        var callHandle = Expression.Call(
            handlerVar,
            handleMethod,
            Expression.Convert(requestParam, requestType),
            cancellationTokenParam);
        var lambda = Expression.Lambda<Func<IServiceProvider, object, CancellationToken, Task<TResponse>>>(
            Expression.Block(
                new[] { handlerVar },
                assignHandler,
                callHandle),
            serviceProviderParam,
            requestParam,
            cancellationTokenParam);
        return lambda.Compile();
    }
    private static Func<IServiceProvider, object, CancellationToken, Task> BuildVoidRequestInvoker(Type requestType)
    {
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
        var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        var requestParam = Expression.Parameter(typeof(object), "request");
        var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var getRequiredServiceMethod = typeof(ServiceProviderServiceExtensions)
            .GetMethod(nameof(ServiceProviderServiceExtensions.GetRequiredService),BindingFlags.Static | BindingFlags.Public)
            .MakeGenericMethod(handlerType);

        var getServiceCall = Expression.Call(
            null, // static method
            getRequiredServiceMethod,
            serviceProviderParam);

        var handlerVar = Expression.Variable(handlerType, "handler");
        var assignHandler = Expression.Assign(handlerVar, getServiceCall);
        var handleMethod = handlerType.GetMethod("Handle");
        var callHandle = Expression.Call(
            handlerVar,
            handleMethod,
            Expression.Convert(requestParam, requestType),
            cancellationTokenParam);
        var lambda = Expression.Lambda<Func<IServiceProvider, object, CancellationToken, Task>>(
            Expression.Block(
                new[] { handlerVar },
                assignHandler,
                callHandle),
            serviceProviderParam,
            requestParam,
            cancellationTokenParam);
        return lambda.Compile();
    }
}


public interface IEndpoint
{
    void AddRoutes(IEndpointRouteBuilder app);
}

public static class MediatorExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        //services.AddSingleton<IMediator, Mediator>();
        services.AddScoped<IMediator, Mediator>();
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


        return services;
    }

    public static IApplicationBuilder UseMediatorEndpoints(this IApplicationBuilder app)
    {
        var endpoints = app.ApplicationServices.GetServices<IEndpoint>();
        var routeBuilder = app.ApplicationServices.GetRequiredService<IEndpointRouteBuilder>();
        foreach (var endpoint in endpoints)
        {
            endpoint.AddRoutes(routeBuilder);
        }
        return app;
    }
}