namespace RoyalStore.Domain.Common;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Unexpected,
    RateLimit
}
