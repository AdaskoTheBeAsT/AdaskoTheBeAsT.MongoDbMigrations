using System;
using System.Runtime.InteropServices;

namespace AdaskoTheBeAsT.MongoDbMigrations.SourceGenerators;

/// <summary>
/// Version information extracted from migration.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal readonly struct VersionInfo
    : IComparable<VersionInfo>,
        IEquatable<VersionInfo>
{
    public VersionInfo(int major, int minor, int revision)
    {
        Major = major;
        Minor = minor;
        Revision = revision;
    }

    public int Major { get; }

    public int Minor { get; }

    public int Revision { get; }

    public static bool operator ==(VersionInfo a, VersionInfo b) => a.Equals(b);

    public static bool operator !=(VersionInfo a, VersionInfo b) => !a.Equals(b);

    public static bool operator >(
        VersionInfo a,
        VersionInfo b) =>
        a.Major > b.Major
        || (a.Major == b.Major && a.Minor > b.Minor)
        || (a.Major == b.Major && a.Minor == b.Minor && a.Revision > b.Revision);

    public static bool operator >=(
        VersionInfo a,
        VersionInfo b) =>
        a.Major > b.Major
        || (a.Major == b.Major && a.Minor > b.Minor)
        || (a.Major == b.Major && a.Minor == b.Minor && a.Revision >= b.Revision);

    public static bool operator <(
        VersionInfo a,
        VersionInfo b) =>
        a.Major < b.Major
        || (a.Major == b.Major && a.Minor < b.Minor)
        || (a.Major == b.Major && a.Minor == b.Minor && a.Revision < b.Revision);

    public static bool operator <=(
        VersionInfo a,
        VersionInfo b) =>
        a.Major < b.Major
        || (a.Major == b.Major && a.Minor < b.Minor)
        || (a.Major == b.Major && a.Minor == b.Minor && a.Revision <= b.Revision);

    public int CompareTo(VersionInfo other)
    {
        if (Equals(other))
        {
            return 0;
        }

        return this > other ? 1 : -1;
    }

    public bool Equals(VersionInfo other)
    {
        return Major == other.Major && Minor == other.Minor && Revision == other.Revision;
    }

    public override bool Equals(object? obj)
    {
        return obj is VersionInfo other && Equals(other);
    }

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

    public override string ToString() => $"{Major}.{Minor}.{Revision}";
}
