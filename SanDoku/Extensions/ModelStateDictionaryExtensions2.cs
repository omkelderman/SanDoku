using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SanDoku.Extensions;

// called 2 cuz the core lib already has one lol
public static class ModelStateDictionaryExtensions2
{
    public static string ToErrorMessage(this ModelStateDictionary modelStateDictionary)
    {
        var errors = modelStateDictionary
            .Where(kvp => kvp.Value!.ValidationState == ModelValidationState.Invalid)
            .Select(kvp =>
            {
                var (key, value) = kvp;
                var errorMessages = value!.Errors.Select(e => e.ErrorMessage);
                return $"{key}=[{string.Join(", ", errorMessages)}]";
            });
        return string.Join(", ", errors);
    }
}