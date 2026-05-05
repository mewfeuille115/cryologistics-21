using Microsoft.AspNetCore.Routing;

namespace Cryologistics21.Common.Presentation.Endpoints;

public interface IEndpoint
{
	void MapEndpoint(IEndpointRouteBuilder app);
}
