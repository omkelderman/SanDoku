using System.Numerics;

namespace SanDoku.Extensions;

public static class NumberBaseExtensions
{
    public static T? NaNOrInfinityToNull<T>(this T d) where T : struct, INumberBase<T>
    {
        return T.IsNaN(d) || T.IsInfinity(d) ? null : d;
    }
}