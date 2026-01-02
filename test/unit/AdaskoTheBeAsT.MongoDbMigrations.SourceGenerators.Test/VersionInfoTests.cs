using AwesomeAssertions;
using Xunit;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators.Test;

public class VersionInfoTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var versionInfo = new VersionInfo(1, 2, 3);

        versionInfo.Major.Should().Be(1);
        versionInfo.Minor.Should().Be(2);
        versionInfo.Revision.Should().Be(3);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var versionInfo = new VersionInfo(1, 2, 3);

        versionInfo.ToString().Should().Be("1.2.3");
    }

    [Theory]
    [InlineData(0, 0, 0, "0.0.0")]
    [InlineData(1, 0, 0, "1.0.0")]
    [InlineData(99, 99, 99, "99.99.99")]
    public void ToString_WithDifferentValues_ShouldFormatCorrectly(int major, int minor, int revision, string expected)
    {
        var versionInfo = new VersionInfo(major, minor, revision);

        versionInfo.ToString().Should().Be(expected);
    }

    [Fact]
    public void CompareTo_WithSameVersion_ShouldReturnZero()
    {
        var v1 = new VersionInfo(1, 2, 3);
        var v2 = new VersionInfo(1, 2, 3);

        v1.CompareTo(v2).Should().Be(0);
    }

    [Fact]
    public void CompareTo_WithLargerMajor_ShouldReturnPositive()
    {
        var v1 = new VersionInfo(2, 0, 0);
        var v2 = new VersionInfo(1, 0, 0);

        v1.CompareTo(v2).Should().BePositive();
    }

    [Fact]
    public void CompareTo_WithSmallerMajor_ShouldReturnNegative()
    {
        var v1 = new VersionInfo(1, 0, 0);
        var v2 = new VersionInfo(2, 0, 0);

        v1.CompareTo(v2).Should().BeNegative();
    }

    [Fact]
    public void CompareTo_WithLargerMinor_ShouldReturnPositive()
    {
        var v1 = new VersionInfo(1, 2, 0);
        var v2 = new VersionInfo(1, 1, 0);

        v1.CompareTo(v2).Should().BePositive();
    }

    [Fact]
    public void CompareTo_WithLargerRevision_ShouldReturnPositive()
    {
        var v1 = new VersionInfo(1, 1, 2);
        var v2 = new VersionInfo(1, 1, 1);

        v1.CompareTo(v2).Should().BePositive();
    }

    [Fact]
    public void EqualityOperator_WithEqualVersions_ShouldReturnTrue()
    {
        var v1 = new VersionInfo(1, 2, 3);
        var v2 = new VersionInfo(1, 2, 3);

        (v1 == v2).Should().BeTrue();
    }

    [Fact]
    public void EqualityOperator_WithDifferentVersions_ShouldReturnFalse()
    {
        var v1 = new VersionInfo(1, 2, 3);
        var v2 = new VersionInfo(1, 2, 4);

        (v1 == v2).Should().BeFalse();
    }

    [Fact]
    public void InequalityOperator_WithDifferentVersions_ShouldReturnTrue()
    {
        var v1 = new VersionInfo(1, 2, 3);
        var v2 = new VersionInfo(1, 2, 4);

        (v1 != v2).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_WithEqualVersions_ShouldReturnFalse()
    {
        var v1 = new VersionInfo(1, 2, 3);
        var v2 = new VersionInfo(1, 2, 3);

        (v1 != v2).Should().BeFalse();
    }

    [Theory]
    [InlineData(2, 0, 0, 1, 0, 0, true)]
    [InlineData(1, 2, 0, 1, 1, 0, true)]
    [InlineData(1, 1, 2, 1, 1, 1, true)]
    [InlineData(1, 0, 0, 2, 0, 0, false)]
    [InlineData(1, 1, 1, 1, 1, 1, false)]
    public void GreaterThanOperator_ShouldReturnExpectedResult(
        int major1,
        int minor1,
        int rev1,
        int major2,
        int minor2,
        int rev2,
        bool expected)
    {
        var v1 = new VersionInfo(major1, minor1, rev1);
        var v2 = new VersionInfo(major2, minor2, rev2);

        (v1 > v2).Should().Be(expected);
    }

    [Theory]
    [InlineData(2, 0, 0, 1, 0, 0, true)]
    [InlineData(1, 1, 1, 1, 1, 1, true)]
    [InlineData(1, 2, 0, 1, 1, 0, true)]
    [InlineData(1, 1, 2, 1, 1, 1, true)]
    [InlineData(1, 0, 0, 2, 0, 0, false)]
    public void GreaterThanOrEqualOperator_ShouldReturnExpectedResult(
        int major1,
        int minor1,
        int rev1,
        int major2,
        int minor2,
        int rev2,
        bool expected)
    {
        var v1 = new VersionInfo(major1, minor1, rev1);
        var v2 = new VersionInfo(major2, minor2, rev2);

        (v1 >= v2).Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 0, 0, 2, 0, 0, true)]
    [InlineData(1, 1, 0, 1, 2, 0, true)]
    [InlineData(1, 1, 1, 1, 1, 2, true)]
    [InlineData(2, 0, 0, 1, 0, 0, false)]
    [InlineData(1, 1, 1, 1, 1, 1, false)]
    public void LessThanOperator_ShouldReturnExpectedResult(
        int major1,
        int minor1,
        int rev1,
        int major2,
        int minor2,
        int rev2,
        bool expected)
    {
        var v1 = new VersionInfo(major1, minor1, rev1);
        var v2 = new VersionInfo(major2, minor2, rev2);

        (v1 < v2).Should().Be(expected);
    }

    [Theory]
    [InlineData(1, 0, 0, 2, 0, 0, true)]
    [InlineData(1, 1, 1, 1, 1, 1, true)]
    [InlineData(1, 1, 0, 1, 2, 0, true)]
    [InlineData(1, 1, 1, 1, 1, 2, true)]
    [InlineData(2, 0, 0, 1, 0, 0, false)]
    public void LessThanOrEqualOperator_ShouldReturnExpectedResult(
        int major1,
        int minor1,
        int rev1,
        int major2,
        int minor2,
        int rev2,
        bool expected)
    {
        var v1 = new VersionInfo(major1, minor1, rev1);
        var v2 = new VersionInfo(major2, minor2, rev2);

        (v1 <= v2).Should().Be(expected);
    }

    [Fact]
    public void Equals_WithVersionInfoObject_ShouldReturnTrue()
    {
        var v1 = new VersionInfo(1, 2, 3);
        object v2 = new VersionInfo(1, 2, 3);

        v1.Equals(v2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentVersionInfoObject_ShouldReturnFalse()
    {
        var v1 = new VersionInfo(1, 2, 3);
        object v2 = new VersionInfo(1, 2, 4);

        v1.Equals(v2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var v1 = new VersionInfo(1, 2, 3);

        v1.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        var v1 = new VersionInfo(1, 2, 3);

        v1.Equals("1.2.3").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentValue()
    {
        var v1 = new VersionInfo(1, 2, 3);
        var v2 = new VersionInfo(1, 2, 3);

        v1.GetHashCode().Should().Be(v2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentVersions_ShouldReturnDifferentValues()
    {
        var v1 = new VersionInfo(1, 2, 3);
        var v2 = new VersionInfo(1, 2, 4);

        v1.GetHashCode().Should().NotBe(v2.GetHashCode());
    }
}
