using Borgonha.Shared.Primitives;

namespace Borgonha.CQRS.Abstractions;

public interface ICommand : IRequest<Result> { }

public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }
