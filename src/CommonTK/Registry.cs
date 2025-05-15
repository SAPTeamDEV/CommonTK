using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Represents a registry that stores resources identified by their locations.
    /// </summary>
    /// <typeparam name="TResource">
    /// The type of the resources stored in the registry.
    /// </typeparam>
    public abstract class Registry<TResource> : IReadOnlyDictionary<ResourceLocation, TResource>, IDisposable
        where TResource : notnull
    {
        private bool disposedValue;

        /// <summary>
        /// Gets the dictionary of resources.
        /// </summary>
        protected Dictionary<ResourceLocation, TResource> Resources { get; } = new();

        /// <summary>
        /// Gets a value indicating whether the registry has been disposed.
        /// </summary>
        public bool Disposed => disposedValue;

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
        public IEnumerator<KeyValuePair<ResourceLocation, TResource>> GetEnumerator() => Resources.GetEnumerator();

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
        public bool TryGetValue(ResourceLocation key, out TResource value) => Resources.TryGetValue(key, out value);

        /// <summary>
        /// Gets the enumerator that iterates through the registered resources.
        /// </summary>
        /// <returns>
        /// An enumerator that iterates through the registered resources.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Resources).GetEnumerator();

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
                throw new ObjectDisposedException(nameof(Registry<TResource>), "The registry has been disposed.");

            if (Resources.ContainsKey(key))
                return false;
            
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
            if (!disposedValue)
            {
                if (disposing)
                {
                    Resources.Clear();
                }

                disposedValue = true;
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
}
