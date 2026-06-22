using SkyRoute.Domain.Enums;

namespace SkyRoute.Domain.Services;

public static class RouteClassifier
{
    public static DocumentType RequiredDocument(string originCountryCode, string destinationCountryCode)
        => string.Equals(originCountryCode, destinationCountryCode, StringComparison.OrdinalIgnoreCase)
            ? DocumentType.NationalId
            : DocumentType.Passport;
}
