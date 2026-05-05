using Cryologistics21.Common.Domain;

namespace Cryologistics21.Common.Application.Exceptions;

public sealed class Cryologistics21Exception : Exception
{
	public Cryologistics21Exception(
		string requestName,
		Error? error = default,
		Exception? innerException = default)
		: base("Application exception", innerException)
	{
		RequestName = requestName;
		Error = error;
	}

	public string RequestName { get; }
	public Error? Error { get; }
}
