using System;
using System.Collections.Generic;
using System.Linq;

namespace SAPTeam.CommonTK;

public class Node : Member
{
    private readonly Dictionary<string, Member> _members = new(StringComparer.OrdinalIgnoreCase);

    public IEnumerable<Member> Members => _members.Values;

    public IEnumerable<Node> Nodes => Members.OfType<Node>();

    public Node(Node? parent, string name) : base(parent, name)
    {

    }

    public bool AddMember(Member member)
    {
        if (member == null)
        {
            throw new ArgumentNullException(nameof(member));
        }

        if (_members.ContainsKey(member.Name))
        {
            return false;
        }

        _members[member.Name] = member;
        return true;
    }

    public Member GetMember(string path) => GetMember(ParsePath(path));

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

    public Node CreateNode(string path) => CreateNode(ParsePath(path));

    protected Node CreateNode(string[] parts)
    {
        if (parts.Length > 1)
        {
            Node subNode = CreateNode([parts[0]]);
            return subNode.CreateNode(parts.Skip(1).ToArray());
        }

        if (!TryGetNode(parts[0], out Node? node))
        {
            node = new Node(this, parts[0]);
            AddMember(node);
        }

        return node;
    }

    public Node GetNode(string path) => GetNode(ParsePath(path));

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

    public IEnumerable<Member> GetMembers()
    {
        foreach (Member member in _members.Values)
        {
            yield return member;

            if (member is Node node)
            {
                foreach (Member child in node.GetMembers())
                {
                    yield return child;
                }
            }
        }
    }

    public IEnumerable<Node> GetNodes() => GetMembers().OfType<Node>();
}
