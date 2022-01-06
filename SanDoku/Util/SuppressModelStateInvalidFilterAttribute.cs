using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System;
using SanDoku.Extensions;

namespace SanDoku.Util
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SuppressModelStateInvalidFilterAttribute : Attribute, IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            // ugly reflection hack, the type is internal so we cannot reference it here
            // shouldn't matter too much tho, since this code only runs once, and not per request
            action.Filters.RemoveAt(x => x.GetType().Name.StartsWith("ModelStateInvalid"));
        }
    }
}