using System;
using System.Collections.Generic;
using System.Linq;

namespace SAPTeam.CommonTK;

/// <summary>
/// Represents a node in a hierarchical structure.
/// </summary>
public class Node : Member
{
    private readonly Dictionary<string, Member> _members = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the members of this node.
    /// </summary>
    public IEnumerable<Member> Members => _members.Values;

    /// <summary>
    /// Gets the child nodes of this node.
    /// </summary>
    public IEnumerable<Node> Nodes => Members.OfType<Node>();

    /// <summary>
    /// Initializes a new instance of the <see cref="Node"/> class.
    /// </summary>
    /// <param name="parent">
    /// The parent node of this node. If null, this node is considered as root.
    /// </param>
    /// <param name="name">
    /// The name of the node.
    /// </param>
    public Node(Node? parent, string name) : base(parent, name)
    {

    }

    /// <summary>
    /// Adds a member to this node.
    /// </summary>
    /// <param name="member">
    /// The member to add.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the member was added successfully and <see langword="false"/> if it already exists.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public bool AddMember(Member member)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        if (member.PathSeparator != PathSeparator)
        {
            throw new ArgumentException("Invalid path separator", nameof(member));
        }

        if (member.Name.Contains(PathSeparator))
        {
            throw new ArgumentException("Member name cannot contain '.'", nameof(member));
        }

        if (_members.ContainsKey(member.Name))
        {
            return false;
        }

        _members[member.Name] = member;
        return true;
    }

    /// <summary>
    /// Gets a member by its path.
    /// </summary>
    /// <param name="path">
    /// The relative path to the member.
    /// </param>
    /// <returns>
    /// The member at the specified path.
    /// </returns>
    public Member GetMember(string path) => GetMember(ParsePath(path));

    /// <summary>
    /// Gets a member by its path components.
    /// </summary>
    /// <param name="parts">
    /// The components of the relative path to the member.
    /// </param>
    /// <returns>
    /// The member at the specified path.
    /// </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    protected Member GetMember(string[] parts)
    {
        if (parts.Length > 1)
        {
            Node node = GetNode([parts[0]]);
            return node.GetMember(parts.Skip(1).ToArray());
        }

        return !_members.TryGetValue(parts[0], out Member? member)
            ? throw new KeyNotFoundException($"Member '{parts[0]}' not found under '{FullPath}'")
            : member;
    }

    /// <summary>
    /// Tries to get a member by its path.
    /// </summary>
    /// <param name="path">
    /// The relative path to the member.
    /// </param>
    /// <param name="member">
    /// The member at the specified path, if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the member was found; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetMember(string path, out Member member)
    {
        try
        {
            member = GetMember(path);
            return true;
        }
        catch
        {
            member = null!;
            return false;
        }
    }

    /// <summary>
    /// Creates a new node at the specified path.
    /// </summary>
    /// <param name="path">
    /// The relative path to the new node.
    /// </param>
    /// <returns>
    /// The new or existing node at the specified path.
    /// </returns>
    public Node CreateNode(string path) => CreateNode(ParsePath(path));

    /// <summary>
    /// Creates a new node at the specified path components.
    /// </summary>
    /// <param name="parts">
    /// The components of the relative path to the new node.
    /// </param>
    /// <returns>
    /// The new or existing node at the specified path.
    /// </returns>
    protected Node CreateNode(string[] parts)
    {
        if (parts.Length > 1)
        {
            Node subNode = CreateNode([parts[0]]);
            return subNode.CreateNode(parts.Skip(1).ToArray());
        }

        if (!TryGetNode(parts[0], out Node? node))
        {
            node = NewNode(parts[0]);
            AddMember(node);
        }

        return node;
    }

    /// <summary>
    /// Creates a new node with the specified name.
    /// Derived classes can override this method to create custom node types.
    /// </summary>
    /// <param name="name">
    /// The name of the new node.
    /// </param>
    /// <returns>
    /// A new Node with the specified name.
    /// </returns>
    protected virtual Node NewNode(string name) => new(this, name);

    /// <summary>
    /// Gets a node by its path.
    /// </summary>
    /// <param name="path">
    /// The relative path to the node.
    /// </param>
    /// <returns>
    /// The node at the specified path.
    /// </returns>
    public Node GetNode(string path) => GetNode(ParsePath(path));

    /// <summary>
    /// Gets a node by its path components.
    /// </summary>
    /// <param name="parts">
    /// The components of the relative path to the node.
    /// </param>
    /// <returns>
    /// The node at the specified path.
    /// </returns>
    /// <exception cref="InvalidCastException"></exception>
    protected Node GetNode(string[] parts)
    {
        if (parts.Length > 1)
        {
            Node subNode = GetNode([parts[0]]);
            return subNode.GetNode(parts.Skip(1).ToArray());
        }

        Node? node = GetMember(parts[0]) as Node;

        return node ?? throw new InvalidCastException($"Member '{parts[0]}' under '{FullPath}' is not a node");
    }

    /// <summary>
    /// Tries to get a node by its path.
    /// </summary>
    /// <param name="path">
    /// The relative path to the node.
    /// </param>
    /// <param name="node">
    /// The node at the specified path, if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the node was found; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetNode(string path, out Node node)
    {
        try
        {
            node = GetNode(path);
            return true;
        }
        catch
        {
            node = null!;
            return false;
        }
    }

    /// <summary>
    /// Gets all members of this node and its child nodes.
    /// </summary>
    /// <returns>
    /// An enumerable collection of all members in this node and its child nodes.
    /// </returns>
    public IEnumerable<Member> GetAllMembers()
    {
        foreach (Member member in _members.Values)
        {
            yield return member;

            if (member is Node node)
            {
                foreach (Member child in node.GetAllMembers())
                {
                    yield return child;
                }
            }
        }
    }

    /// <summary>
    /// Gets all child nodes recursively.
    /// </summary>
    /// <returns>
    /// An enumerable collection of all nodes in this node recursively.
    /// </returns>
    public IEnumerable<Node> GetAllNodes() => GetAllMembers().OfType<Node>();
}
