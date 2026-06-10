using Borgonha.Shared.Primitives;

namespace Borgonha.CQRS.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
