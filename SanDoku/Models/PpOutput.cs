namespace SanDoku.Models;

public record PpOutput
(
    double? Pp,
    List<PpDisplayAttribute> Attributes
);