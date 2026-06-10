using Borgonha.Shared.Primitives;

namespace Borgonha.CQRS.Abstractions;

public interface IQueryHandler<TQuery, TResponse> where TQuery : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
