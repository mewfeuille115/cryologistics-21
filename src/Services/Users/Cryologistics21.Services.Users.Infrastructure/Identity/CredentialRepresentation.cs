namespace Cryologistics21.Services.Users.Infrastructure.Identity;

internal sealed record CredentialRepresentation(string Type, string Value, bool Temporary);
