using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace PandesalExpress.Infrastructure.Abstractions;

public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("Handle") 
            ?? throw new InvalidOperationException($"Handler for {command.GetType()} does not contain a Handle method");

        var task = (Task<TResponse>)method.Invoke(handler, new object[] { command, cancellationToken })!;
        return await task;
    }

    public async Task<TResponse> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handler = _serviceProvider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod("Handle")
            ?? throw new InvalidOperationException($"Handler for {query.GetType()} does not contain a Handle method");

        var task = (Task<TResponse>)method.Invoke(handler, new object[] { query, cancellationToken })!;
        return await task;
    }
}
