namespace SanDoku.Models;

public record PpOutput
(
    double? Pp,
    Dictionary<string, double?> ExtraValues
);