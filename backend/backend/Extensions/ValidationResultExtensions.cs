using FluentValidation.Results;

namespace backend.Extensions;

public static class ValidationResultExtensions
{
    public static Dictionary<string, string[]> ToValidationProblemDictionary(
        this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray()
            );
    }
}