namespace Borgonha.Domain.Errors;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    UnprocessableEntity,
    Unexpected
}
