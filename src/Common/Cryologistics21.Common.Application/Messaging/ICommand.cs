using System.Diagnostics.CodeAnalysis;

namespace Cryologistics21.Common.Application.Messaging;

[SuppressMessage(
	"Microsoft.Design",
	"CA1040:AvoidEmptyInterfaces",
	Justification = "Intentional marker used to constrain command handlers and pipeline decorators.")]
public interface ICommand;

[SuppressMessage(
	"SonarAnalyzer.CSharp",
	"S2326",
	Justification = "TResponse links this command marker to its handler response type.")]
[SuppressMessage(
	"Microsoft.Design",
	"CA1040:AvoidEmptyInterfaces",
	Justification = "Intentional marker linking a command to its response type.")]
public interface ICommand<TResponse>;
