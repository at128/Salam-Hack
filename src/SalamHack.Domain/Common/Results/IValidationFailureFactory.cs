namespace SalamHack.Domain.Common.Results;

public interface IValidationFailureFactory<TSelf>
{
    static abstract TSelf FromValidationErrors(IReadOnlyCollection<Error> errors);
}
