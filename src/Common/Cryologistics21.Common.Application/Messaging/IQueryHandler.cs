using Cryologistics21.Common.Domain;

namespace Cryologistics21.Common.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse>
	where TQuery : IQuery<TResponse>
{
	Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}
