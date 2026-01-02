using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions.Test;

public class VersionTests
{
    [Theory]
    [InlineData("1.0.0", 1, 0, 0)]
    [InlineData("2.5.3", 2, 5, 3)]
    [InlineData("0.0.0", 0, 0, 0)]
    [InlineData("99.99.99", 99, 99, 99)]
    public void Constructor_WithValidString_ShouldParseCorrectly(string versionString, int major, int minor, int revision)
    {
        var version = new Version(versionString);

        version.Major.Should().Be(major);
        version.Minor.Should().Be(minor);
        version.Revision.Should().Be(revision);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(2, 5, 3)]
    [InlineData(0, 0, 0)]
    public void Constructor_WithIntegers_ShouldSetCorrectly(int major, int minor, int revision)
    {
        var version = new Version(major, minor, revision);

        version.Major.Should().Be(major);
        version.Minor.Should().Be(minor);
        version.Revision.Should().Be(revision);
    }

    [Fact]
    public void Constructor_WithTooManyParts_ShouldThrowArgumentException()
    {
        var act = () => new Version("1.2.3.4");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithTooFewParts_ShouldThrowArgumentException()
    {
        var act = () => new Version("1.2");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithInvalidPart_ShouldThrowArgumentException()
    {
        var act = () => new Version("1.abc.0");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Zero_ShouldReturnVersionWithAllZeros()
    {
        var version = Version.Zero();

        version.Major.Should().Be(0);
        version.Minor.Should().Be(0);
        version.Revision.Should().Be(0);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var version = new Version(1, 2, 3);

        version.ToString().Should().Be("1.2.3");
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0", true)]
    [InlineData("1.0.0", "1.0.1", false)]
    [InlineData("1.0.0", "2.0.0", false)]
    public void Equals_ShouldCompareCorrectly(string a, string b, bool expected)
    {
        var versionA = new Version(a);
        var versionB = new Version(b);

        versionA.Equals(versionB).Should().Be(expected);
        (versionA == versionB).Should().Be(expected);
        (versionA != versionB).Should().Be(!expected);
    }

    [Theory]
    [InlineData("2.0.0", "1.0.0", true)]
    [InlineData("1.1.0", "1.0.0", true)]
    [InlineData("1.0.1", "1.0.0", true)]
    [InlineData("1.0.0", "1.0.0", false)]
    [InlineData("1.0.0", "2.0.0", false)]
    public void GreaterThan_ShouldCompareCorrectly(string a, string b, bool expected)
    {
        var versionA = new Version(a);
        var versionB = new Version(b);

        (versionA > versionB).Should().Be(expected);
    }

    [Theory]
    [InlineData("1.0.0", "2.0.0", true)]
    [InlineData("1.0.0", "1.1.0", true)]
    [InlineData("1.0.0", "1.0.1", true)]
    [InlineData("1.0.0", "1.0.0", false)]
    [InlineData("2.0.0", "1.0.0", false)]
    public void LessThan_ShouldCompareCorrectly(string a, string b, bool expected)
    {
        var versionA = new Version(a);
        var versionB = new Version(b);

        (versionA < versionB).Should().Be(expected);
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0", true)]
    [InlineData("1.0.0", "2.0.0", true)]
    [InlineData("2.0.0", "1.0.0", false)]
    public void LessThanOrEqual_ShouldCompareCorrectly(string a, string b, bool expected)
    {
        var versionA = new Version(a);
        var versionB = new Version(b);

        (versionA <= versionB).Should().Be(expected);
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0", true)]
    [InlineData("2.0.0", "1.0.0", true)]
    [InlineData("1.0.0", "2.0.0", false)]
    public void GreaterThanOrEqual_ShouldCompareCorrectly(string a, string b, bool expected)
    {
        var versionA = new Version(a);
        var versionB = new Version(b);

        (versionA >= versionB).Should().Be(expected);
    }

    [Theory]
    [InlineData("1.0.0", "1.0.0", 0)]
    [InlineData("2.0.0", "1.0.0", 1)]
    [InlineData("1.0.0", "2.0.0", -1)]
    public void CompareTo_ShouldReturnCorrectValue(string a, string b, int expected)
    {
        var versionA = new Version(a);
        var versionB = new Version(b);

        versionA.CompareTo(versionB).Should().Be(expected);
    }

    [Fact]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        Version version = "1.2.3";

        version.Major.Should().Be(1);
        version.Minor.Should().Be(2);
        version.Revision.Should().Be(3);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        var version = new Version(1, 2, 3);
        string versionString = version;

        versionString.Should().Be("1.2.3");
    }

    [Fact]
    public void GetHashCode_SameVersions_ShouldReturnSameHashCode()
    {
        var version1 = new Version(1, 2, 3);
        var version2 = new Version(1, 2, 3);

        version1.GetHashCode().Should().Be(version2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentVersions_ShouldReturnDifferentHashCode()
    {
        var version1 = new Version(1, 2, 3);
        var version2 = new Version(1, 2, 4);

        version1.GetHashCode().Should().NotBe(version2.GetHashCode());
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var version = new Version(1, 0, 0);

#pragma warning disable CA1508
        version.Equals((object?)null).Should().BeFalse();
#pragma warning restore CA1508
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        var version = new Version(1, 0, 0);

        version.Equals(42).Should().BeFalse();
    }
}
