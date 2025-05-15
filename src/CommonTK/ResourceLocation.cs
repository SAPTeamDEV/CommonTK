// ----------------------------------------------------------------------------
//  <copyright file="ResourceLocation.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK;

/// <summary>
/// Represents a location of a resource in the registry.
/// </summary>
public readonly struct ResourceLocation : IEquatable<ResourceLocation>
{
    /// <summary>
    /// Gets the domain of the resource.
    /// </summary>
    public string Domain { get; }

    /// <summary>
    /// Gets the path of the resource.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// Initializes a new <see cref="ResourceLocation"/>.
    /// </summary>
    /// <param name="domain">
    /// The domain of the resource.
    /// </param>
    /// <param name="path">
    /// The path of the resource.
    /// </param>
    public ResourceLocation(string domain, string path)
    {
        Domain = domain;
        Path = path;
    }

    /// <summary>
    /// Checks if this resource location is equal to another resource location.
    /// </summary>
    /// <param name="other">
    /// The other resource location to compare with.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the resource locations are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(ResourceLocation other)
    {
        return string.Equals(Domain, other.Domain, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ResourceLocation other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(Domain),
            StringComparer.OrdinalIgnoreCase.GetHashCode(Path));
    }

    /// <summary>
    /// Compares two <see cref="ResourceLocation"/> instances for equality.
    /// </summary>
    /// <param name="left">
    /// The left <see cref="ResourceLocation"/> instance.
    /// </param>
    /// <param name="right">
    /// The right <see cref="ResourceLocation"/> instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the two <see cref="ResourceLocation"/> instances are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(ResourceLocation left, ResourceLocation right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="ResourceLocation"/> instances for inequality.
    /// </summary>
    /// <param name="left">
    /// The left <see cref="ResourceLocation"/> instance.
    /// </param>
    /// <param name="right">
    /// The right <see cref="ResourceLocation"/> instance.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the two <see cref="ResourceLocation"/> instances are not equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(ResourceLocation left, ResourceLocation right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => $"{Domain}:{Path}";
}
