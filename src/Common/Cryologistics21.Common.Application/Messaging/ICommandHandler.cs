using Cryologistics21.Common.Domain;

namespace Cryologistics21.Common.Application.Messaging;

public interface ICommandHandler<in TCommand>
	where TCommand : ICommand
{
	Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
	where TCommand : ICommand<TResponse>
{
	Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
