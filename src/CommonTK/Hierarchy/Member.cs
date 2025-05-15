// ----------------------------------------------------------------------------
//  <copyright file="Member.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.Hierarchy;

/// <summary>
/// Represents a member in a hierarchical structure.
/// </summary>
public class Member
{
    /// <summary>
    /// Gets the path separator used to identify members in the hierarchy.
    /// </summary>
    public virtual char PathSeparator => '.';

    /// <summary>
    /// Gets the name of the member.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the full path of the member, including its parent nodes.
    /// </summary>
    public string FullPath { get; }

    /// <summary>
    /// Gets the parent node of this member, or null if this member is a root node.
    /// </summary>
    public Node? Parent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Member"/> class.
    /// </summary>
    /// <param name="parent">
    /// The parent node of this member. If null, this member is considered as root.
    /// </param>
    /// <param name="name">
    /// The name of the member. Root members can have an empty name.
    /// </param>
    public Member(Node? parent, string? name)
    {
        Parent = parent;
        Name = string.IsNullOrEmpty(name) ? string.Empty : name!.Trim();

        if (Name.Contains(PathSeparator))
        {
            throw new ArgumentException($"Member name cannot contain '{PathSeparator}'", nameof(name));
        }

        if (Parent is null)
        {
            FullPath = Name;
        }
        else
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentException("Non-root members must have a name", nameof(name));
            }

            var parentPath = Parent.FullPath;
            FullPath = string.IsNullOrEmpty(parentPath) ? Name : JoinPath(parentPath, Name);
        }
    }

    /// <summary>
    /// Parses the given path into its components.
    /// </summary>
    /// <param name="path">
    /// The path to parse.
    /// </param>
    /// <returns>
    /// An array of strings representing the components of the path.
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    protected string[] ParsePath(string path)
    {
        if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        var parts = path.Split(PathSeparator);

        return parts.Length == 0 ? throw new ArgumentException("Invalid path", nameof(path)) : parts;
    }

    /// <summary>
    /// Creates an standard path from the given components.
    /// </summary>
    /// <param name="parts">
    /// The components of the path.
    /// </param>
    /// <returns>
    /// A string representing the path.
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    protected string CreatePath(string[] parts) => parts.Length == 0 ? throw new ArgumentException("Invalid path", nameof(parts)) : JoinPath(parts);

    /// <summary>
    /// Concatenates the given paths into a single path.
    /// </summary>
    /// <param name="paths">
    /// The paths to join.
    /// </param>
    /// <returns>
    /// A string representing the joined path.
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    protected string JoinPath(params string[] paths)
    {
        return paths.Length == 0
            ? throw new ArgumentException("Path cannot be null or empty", nameof(paths))
            : string.Join(PathSeparator.ToString(), paths);
    }

    /// <summary>
    /// Converts the given path to an absolute path based on the current member's full path.
    /// </summary>
    /// <param name="path">
    /// The relative path to convert.
    /// </param>
    /// <returns>
    /// A string representing the absolute path.
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    protected string ToAbsolutePath(string path) => ToAbsolutePath(path, FullPath);

    /// <summary>
    /// Converts the given path to an absolute path based on the specified base path.
    /// </summary>
    /// <param name="path">
    /// The relative path to convert.
    /// </param>
    /// <param name="basePath">
    /// The base path to use for conversion.
    /// </param>
    /// <returns>
    /// A string representing the absolute path.
    /// </returns>
    /// <exception cref="ArgumentException"></exception>
    protected string ToAbsolutePath(string path, string basePath)
    {
        return string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path)
            ? throw new ArgumentException("Path cannot be null or empty", nameof(path))
            : string.IsNullOrEmpty(basePath) || string.IsNullOrWhiteSpace(basePath)
            ? throw new ArgumentException("Base path cannot be null or empty", nameof(basePath))
            : path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase) ? path : JoinPath(basePath, path);
    }
}
