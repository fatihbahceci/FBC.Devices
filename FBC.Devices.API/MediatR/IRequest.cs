using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace FBC.Devices.API.MediatR;

public interface IRequest<out TResponse> : IMessage
{
}
public interface IRequest : IMessage
{
}
public interface IMessage
{
}
public interface IRequestHandler<in TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    TResponse Handle(TRequest request);
}
public interface IRequestHandler<in TRequest> 
    where TRequest : IRequest
{
    void Handle(TRequest request);
}
public static class RequestHandlerExtensions
{
    public static MethodInfo GetHandleMethodInfo<TRequest, TResponse>(this IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        return handler.GetType().GetMethod("Handle", new Type[] { typeof(TRequest) })!;
    }
    public static MethodInfo GetHandleMethodInfo<TRequest>(this IRequestHandler<TRequest> handler)
        where TRequest : IRequest
    {
        return handler.GetType().GetMethod("Handle", new Type[] { typeof(TRequest) })!;
    }
}

public static class RequestExtensions
{
    public static Type GetResponseType<TRequest, TResponse>(this TRequest request)
        where TRequest : IRequest<TResponse>
    {
        return typeof(TResponse);
    }
    public static Type GetResponseType<TRequest>(this TRequest request)
        where TRequest : IRequest
    {
        return typeof(void);
    }
}

public static class RequestTypeExtensions
{
    public static bool IsRequestOfType<TResponse>(this Type requestType)
    {
        if (!typeof(IRequest).IsAssignableFrom(requestType))
            return false;
        if (requestType.IsGenericType && requestType.GetGenericTypeDefinition() == typeof(IRequest<>))
        {
            var genericArgs = requestType.GetGenericArguments();
            return genericArgs.Length == 1 && genericArgs[0] == typeof(TResponse);
        }
        foreach (var iface in requestType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IRequest<>))
            {
                var genericArgs = iface.GetGenericArguments();
                if (genericArgs.Length == 1 && genericArgs[0] == typeof(TResponse))
                    return true;
            }
        }
        return false;
    }
}

public static class RequestTypeResponseExtensions
{
    public static Type GetResponseType(this Type requestType)
    {
        if (!typeof(IRequest).IsAssignableFrom(requestType))
            throw new ArgumentException("Type must implement IRequest interface.", nameof(requestType));
        if (requestType.IsGenericType && requestType.GetGenericTypeDefinition() == typeof(IRequest<>))
        {
            return requestType.GetGenericArguments()[0];
        }
        foreach (var iface in requestType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IRequest<>))
            {
                return iface.GetGenericArguments()[0];
            }
        }
        return typeof(void);
    }
}

public static class RequestHandlerTypeExtensions
{
    public static Type GetRequestType<TRequest, TResponse>(this IRequestHandler<TRequest, TResponse> handler)
        where TRequest : IRequest<TResponse>
    {
        return typeof(TRequest);
    }
    public static Type GetRequestType<TRequest>(this IRequestHandler<TRequest> handler)
        where TRequest : IRequest
    {
        return typeof(TRequest);
    }
}

public interface IEndpoint
{
    void AddRoutes(IEndpointRouteBuilder app);
}

public interface IMediator
{
    TResponse Send<TResponse>(IRequest<TResponse> request);
    void Send(IRequest request);
}
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public TResponse Send<TResponse>(IRequest<TResponse> request)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
            throw new InvalidOperationException($"No handler found for request type {request.GetType().FullName}");
        return handler.Handle((dynamic)request);
    }
    public void Send(IRequest request)
    {
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        dynamic handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
            throw new InvalidOperationException($"No handler found for request type {request.GetType().FullName}");
        handler.Handle((dynamic)request);
    }
}

public static class MediatorExtensions
{
    public static TResponse SendRequest<TResponse>(this IMediator mediator, IRequest<TResponse> request)
    {
        return mediator.Send(request);
    }
    public static void SendRequest(this IMediator mediator, IRequest request)
    {
        mediator.Send(request);
    }
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddSingleton<IMediator, Mediator>();
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
            services.AddTransient(handler.InterfaceType, handler.HandlerType);
        }

        var serviceDescriptors = allAssemblies.SelectMany(assembly => assembly.DefinedTypes.Where(type => type is { IsAbstract: false, IsInterface: false } && type.IsAssignableTo(typeof(IEndpoint))).Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type)).ToArray());
        services.TryAddEnumerable(serviceDescriptors);
        //services.AddApiVersion();
        return services;
    }
    //public static IServiceCollection AddApiVersion(this IServiceCollection services)
    //{
    //    var builder = services.AddApiVersioning(options =>
    //    {
    //        options.DefaultApiVersion = new ApiVersion(1, 0);
    //        options.ReportApiVersions = true;
    //        options.AssumeDefaultVersionWhenUnspecified = true;
    //        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    //    }).AddApiExplorer(options =>
    //    {
    //        options.GroupNameFormat = "'v'VVV";
    //        options.SubstituteApiVersionInUrl = true;
    //    });

    //    return services;
    //}

    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        ApiVersionSet apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(1.0)).HasApiVersion(new ApiVersion(2.0))
            .ReportApiVersions()
            .Build();
        RouteGroupBuilder app2 = app.MapGroup("api/v{v:apiVersion}").WithApiVersionSet(apiVersionSet);
        IEnumerable<IEndpoint> requiredService = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        foreach (IEndpoint item in requiredService)
        {
            item.AddRoutes(app2);
        }

        return app;
    }
}