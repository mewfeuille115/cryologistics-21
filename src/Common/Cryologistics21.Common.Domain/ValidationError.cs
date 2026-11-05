using System.Collections.ObjectModel;

namespace Cryologistics21.Common.Domain;

public sealed record ValidationError : DomainError
{
	private readonly ReadOnlyCollection<DomainError> _errors;

	public ValidationError(DomainError[] errors)
		: base(
			"General.Validation",
			"One or more validation errors occurred",
			ErrorType.Validation)
	{
		_errors = Array.AsReadOnly([.. errors]);
	}

	public IReadOnlyList<DomainError> Errors => _errors;

	public static ValidationError FromResults(IEnumerable<Result> results) =>
		new([.. results.Where(r => r.IsFailure).Select(r => r.Error)]);
}
