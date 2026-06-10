using Borgonha.Shared.Primitives;

namespace Borgonha.CQRS.Abstractions;

public interface IQueryDispatcher
{
    Task<Result<TResponse>> Dispatch<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResponse>;
}
