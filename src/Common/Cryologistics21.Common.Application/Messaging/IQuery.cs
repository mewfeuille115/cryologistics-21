using System.Diagnostics.CodeAnalysis;

namespace Cryologistics21.Common.Application.Messaging;

[SuppressMessage(
	"SonarAnalyzer.CSharp",
	"S2326",
	Justification = "TResponse links this query marker to its handler response type.")]
[SuppressMessage(
	"Microsoft.Design",
	"CA1040:AvoidEmptyInterfaces",
	Justification = "Intentional marker linking a query to its response type.")]
public interface IQuery<TResponse>;
