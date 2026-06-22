using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Services;

namespace SkyRoute.Tests.Domain;

public sealed class RouteClassifierTests
{
    [Fact]
    public void RequiredDocument_SameCountry_ReturnsNationalId()
    {
        var result = RouteClassifier.RequiredDocument("US", "US");
        Assert.Equal(DocumentType.NationalId, result);
    }

    [Fact]
    public void RequiredDocument_DifferentCountries_ReturnsPassport()
    {
        var result = RouteClassifier.RequiredDocument("US", "GB");
        Assert.Equal(DocumentType.Passport, result);
    }

    [Theory]
    [InlineData("us", "US")]
    [InlineData("US", "us")]
    [InlineData("us", "us")]
    public void RequiredDocument_SameCountryCaseInsensitive_ReturnsNationalId(string origin, string destination)
    {
        var result = RouteClassifier.RequiredDocument(origin, destination);
        Assert.Equal(DocumentType.NationalId, result);
    }

    [Fact]
    public void RequiredDocument_DomesticRoute_JfkToLax_IsNationalId()
    {
        // Both airports are in the US
        var result = RouteClassifier.RequiredDocument("US", "US");
        Assert.Equal(DocumentType.NationalId, result);
    }

    [Fact]
    public void RequiredDocument_InternationalRoute_JfkToLhr_IsPassport()
    {
        // JFK is US, LHR is GB
        var result = RouteClassifier.RequiredDocument("US", "GB");
        Assert.Equal(DocumentType.Passport, result);
    }

    [Theory]
    [InlineData("US", "JP")]
    [InlineData("GB", "FR")]
    [InlineData("DE", "SG")]
    [InlineData("CO", "US")]
    public void RequiredDocument_AnyTwoDifferentCountries_ReturnsPassport(string origin, string destination)
    {
        var result = RouteClassifier.RequiredDocument(origin, destination);
        Assert.Equal(DocumentType.Passport, result);
    }

    [Theory]
    [InlineData("GB", "GB")]
    [InlineData("FR", "FR")]
    [InlineData("JP", "JP")]
    public void RequiredDocument_AnyTwoSameCountry_ReturnsNationalId(string origin, string destination)
    {
        var result = RouteClassifier.RequiredDocument(origin, destination);
        Assert.Equal(DocumentType.NationalId, result);
    }
}
