using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace PandesalExpress.Infrastructure.Abstractions;

public class Mediator(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        Type handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        object handler = serviceProvider.GetRequiredService(handlerType);

        MethodInfo method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handler for {command.GetType()} does not contain a Handle method");

        var task = (Task<TResponse>)method.Invoke(handler, [command, cancellationToken])!;
        return await task;
    }

    public async Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        Type handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        object handler = serviceProvider.GetRequiredService(handlerType);

        MethodInfo method = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException($"Handler for {query.GetType()} does not contain a Handle method");

        var task = (Task<TResponse>)method.Invoke(handler, [query, cancellationToken])!;
        return await task;
    }
}
