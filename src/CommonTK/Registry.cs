// ----------------------------------------------------------------------------
//  <copyright file="Registry.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

using System.Collections;

namespace SAPTeam.CommonTK;

/// <summary>
/// Represents a registry that stores resources identified by their locations.
/// </summary>
/// <typeparam name="TResource">
/// The type of the resources stored in the registry.
/// </typeparam>
public class Registry<TResource> : IEnumerable<ResourceLocation>, IDisposable
    where TResource : notnull
{
    Dictionary<string, Dictionary<string, Dictionary<string, TResource>>> _domains = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether the registry has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Gets the resource at the specified location.
    /// </summary>
    /// <param name="key">
    /// The location of the resource.
    /// </param>
    /// <returns>
    /// The resource at the specified location.
    /// </returns>
    public TResource this[ResourceLocation key] => Resources[key];

    /// <summary>
    /// Gets the registered resources locations.
    /// </summary>
    public IEnumerable<ResourceLocation> Keys => Resources.Keys;

    /// <summary>
    /// Gets the registered resources.
    /// </summary>
    public IEnumerable<TResource> Values => Resources.Values;

    /// <summary>
    /// Gets the number of registered resources.
    /// </summary>
    public int Count => Resources.Count;

    /// <summary>
    /// Checks if the registry contains a resource at the specified location.
    /// </summary>
    /// <param name="key">
    /// The location of the resource.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the resource is registered; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ContainsKey(ResourceLocation key) => Resources.ContainsKey(key);

    /// <summary>
    /// Gets an enumerator that iterates through the registered resources.
    /// </summary>
    /// <returns>
    /// An enumerator that iterates through the registered resources.
    /// </returns>
    public IEnumerator<TResource> GetEnumerator() => Resources.GetEnumerator();

    /// <summary>
    /// Tries to get the resource at the specified location.
    /// </summary>
    /// <param name="key">
    /// The location of the resource.
    /// </param>
    /// <param name="value">
    /// The resource at the specified location, if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the resource was found; otherwise, <see langword="false"/>.
    /// </returns>
#if NET6_0_OR_GREATER
    public bool TryGetValue(ResourceLocation key, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out TResource value)
#else
    public bool TryGetValue(ResourceLocation key, out TResource value)
#endif
        => Resources.TryGetValue(key, out value);

    /// <summary>
    /// Gets the enumerator that iterates through the registered resources.
    /// </summary>
    /// <returns>
    /// An enumerator that iterates through the registered resources.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Resources).GetEnumerator();

    /// <summary>
    /// Creates or retrieves a domain within the registry.
    /// </summary>
    /// <remarks>The domain name comparison is case-insensitive. If the specified domain already exists, the
    /// existing domain's resources are returned.</remarks>
    /// <param name="domain">The name of the domain to create or retrieve. This value cannot be null, empty, or consist only of whitespace.</param>
    /// <returns>A dictionary representing the resources associated with the specified domain.  If the domain does not already
    /// exist, a new domain is created and returned.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the registry has been disposed and is no longer usable.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="domain"/> is null, empty, or consists only of whitespace.</exception>
    protected Dictionary<string, Dictionary<string, TResource>> CreateDomain(string domain)
    {
        if (Disposed)
        {
            throw new ObjectDisposedException(nameof(Registry<TResource>), "The registry has been disposed.");
        }

        if (string.IsNullOrEmpty(domain) || string.IsNullOrWhiteSpace(domain))
        {
            throw new ArgumentException("Domain cannot be null or empty.", nameof(domain));
        }

        if (!_domains.TryGetValue(domain, out Dictionary<string, Dictionary<string, TResource>>? domainResources))
        {
            domainResources = new(StringComparer.OrdinalIgnoreCase);
            _domains[domain] = domainResources;
        }

        return domainResources;
    }

    /// <summary>
    /// Tries to add a resource to the registry.
    /// </summary>
    /// <param name="key">
    /// The location of the resource.
    /// </param>
    /// <param name="value">
    /// The resource to add.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the resource was added; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryAdd(ResourceLocation key, TResource value)
    {
        if (Disposed)
        {
            throw new ObjectDisposedException(nameof(Registry<TResource>), "The registry has been disposed.");
        }



        if (Resources.ContainsKey(key))
        {
            return false;
        }

        Resources[key] = value;

        return true;
    }

    /// <summary>
    /// Disposes the registry and releases its resources.
    /// </summary>
    /// <remarks>
    /// After calling this method, the registry resources will be cleared and cannot be used.
    /// </remarks>
    /// <param name="disposing">
    /// A value indicating whether the method was called directly or by the finalizer.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!Disposed)
        {
            if (disposing)
            {
                _domains.Clear();
            }

            Disposed = true;
        }
    }

    /// <summary>
    /// Disposes the registry and releases its resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
