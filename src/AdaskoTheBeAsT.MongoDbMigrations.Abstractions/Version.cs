using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace AdaskoTheBeAsT.MongoDbMigrations.Abstractions;

/// <summary>
/// Semantic versioning for migrations.
/// </summary>
[StructLayout(LayoutKind.Auto)]
#pragma warning disable SA1202 // Elements should be ordered by access
public readonly struct Version : IComparable<Version>, IEquatable<Version>
{
    /// <summary>
    /// Major version number.
    /// </summary>
    public readonly int Major;

    /// <summary>
    /// Minor version number.
    /// </summary>
    public readonly int Minor;

    /// <summary>
    /// Revision number.
    /// </summary>
    public readonly int Revision;

    private const char VersionSplitter = '.';
    private const int MaxLength = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="Version"/> struct from a string.
    /// </summary>
    /// <param name="version">Version string in format "MAJOR.MINOR.REVISION".</param>
    public Version(string version)
    {
        var parts = version.Split(VersionSplitter);

        if (parts.Length > MaxLength)
        {
            throw new ArgumentException($"Version string '{version}' has too many parts. Maximum is {MaxLength}.", nameof(version));
        }

        if (parts.Length < MaxLength)
        {
            throw new ArgumentException($"Version string '{version}' must have exactly {MaxLength} parts (MAJOR.MINOR.REVISION).", nameof(version));
        }

        ParseVersionPart(parts[0], out var major);
        ParseVersionPart(parts[1], out var minor);
        ParseVersionPart(parts[2], out var revision);

        Major = major;
        Minor = minor;
        Revision = revision;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Version"/> struct.
    /// </summary>
    /// <param name="major">Major version number.</param>
    /// <param name="minor">Minor version number.</param>
    /// <param name="revision">Revision number.</param>
    public Version(int major, int minor, int revision)
    {
        Major = major;
        Minor = minor;
        Revision = revision;
    }

    /// <summary>
    /// Implicit conversion from string to Version.
    /// </summary>
    public static implicit operator Version(string version)
    {
        return new Version(version);
    }

    /// <summary>
    /// Implicit conversion from Version to string.
    /// </summary>
    public static implicit operator string(Version version)
    {
        return version.ToString();
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Version a, Version b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Version a, Version b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Greater than operator.
    /// </summary>
    public static bool operator >(Version a, Version b)
    {
        return a.Major > b.Major
               || (a.Major == b.Major && a.Minor > b.Minor)
               || (a.Major == b.Major && a.Minor == b.Minor && a.Revision > b.Revision);
    }

    /// <summary>
    /// Less than operator.
    /// </summary>
    public static bool operator <(Version a, Version b)
    {
        return a != b && !(a > b);
    }

    /// <summary>
    /// Less than or equal operator.
    /// </summary>
    public static bool operator <=(Version a, Version b)
    {
        return a == b || a < b;
    }

    /// <summary>
    /// Greater than or equal operator.
    /// </summary>
    public static bool operator >=(Version a, Version b)
    {
        return a == b || a > b;
    }

    /// <summary>
    /// Creates a zero version (0.0.0).
    /// </summary>
    /// <returns>Zero version.</returns>
    public static Version Zero()
    {
        return new Version(0, 0, 0);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Major}.{Minor}.{Revision}";
    }

    /// <inheritdoc/>
    public int CompareTo(Version other)
    {
        if (Equals(other))
        {
            return 0;
        }

        return this > other ? 1 : -1;
    }

    /// <inheritdoc/>
    public bool Equals(Version other)
    {
        return other.Major == Major && other.Minor == Minor && other.Revision == Revision;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (obj is not Version version)
        {
            return false;
        }

        return Equals(version);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var result = Major;
            result = (result * 397) ^ Minor;
            result = (result * 397) ^ Revision;
            return result;
        }
    }

    private static void ParseVersionPart(string value, out int target)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out target))
        {
            throw new ArgumentException($"Invalid version part: '{value}'. Must be a valid integer.", nameof(value));
        }
    }
}
