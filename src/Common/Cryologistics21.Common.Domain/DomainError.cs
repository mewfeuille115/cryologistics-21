namespace Cryologistics21.Common.Domain;

public record DomainError
{
	public static readonly DomainError None = new(string.Empty, string.Empty, ErrorType.Failure);
	public static readonly DomainError NullValue = new(
		"General.Null",
		"Null value was provided",
		ErrorType.Failure);

	public DomainError(string code, string description, ErrorType type)
	{
		Code = code;
		Description = description;
		Type = type;
	}

	public string Code { get; }
	public string Description { get; }
	public ErrorType Type { get; }

	public static DomainError Failure(string code, string description) =>
		new(code, description, ErrorType.Failure);

	public static DomainError NotFound(string code, string description) =>
		new(code, description, ErrorType.NotFound);

	public static DomainError Problem(string code, string description) =>
		new(code, description, ErrorType.Problem);

	public static DomainError Conflict(string code, string description) =>
		new(code, description, ErrorType.Conflict);
}
