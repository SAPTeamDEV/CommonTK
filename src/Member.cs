using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
    public class Member
    {
        public string Name { get; }

        public string FullPath { get; }

        public Node? Parent { get; }

        public Member(Node? parent, string name)
        {
            Parent = parent;
            Name = name;

            if (Parent is null)
            {
                FullPath = Name;
            }
            else
            {
                var parentPath = Parent.FullPath;
                FullPath = string.IsNullOrEmpty(parentPath) ? Name : string.Join(".", parentPath, Name);
            }
        }

        protected string[] ParsePath(string path)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            }

            var parts = path.Split('.');

            if (parts.Length == 0)
            {
                throw new ArgumentException("Invalid path", nameof(path));
            }

            return parts;
        }
    }
}
