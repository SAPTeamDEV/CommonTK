using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    public class DummyMultiStatusProvider : IMultiStatusProvider
    {
        public List<string> Input { get; }

        public DummyMultiStatusProvider()
        {
            Input = new List<string>();
        }

        public void Clear()
        {
            Input.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        public void Write(string message)
        {
            Input.Add(message);
        }

        public void Clear(string message)
        {
            Input.Remove(message);
        }
    }
}
