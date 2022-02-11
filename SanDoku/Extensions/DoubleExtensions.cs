namespace SanDoku.Extensions
{
    public static class DoubleExtensions
    {
        public static double? NaNOrInfinityToNull(this double d)
        {
            return double.IsNaN(d) || double.IsInfinity(d) ? null : d;
        }
    }
}