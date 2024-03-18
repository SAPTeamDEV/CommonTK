using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    public class DummyMultiStatusProvider : IMultiStatusProvider
    {
        public Dictionary<int, string> Input { get; }

        public DummyMultiStatusProvider()
        {
            Input = new Dictionary<int, string>();
        }

        public void Clear()
        {
            Input.Clear();
        }

        public void Dispose()
        {
            Clear();
        }

        public StatusIdentifier Write(string message)
        {
            var id = StatusIdentifier.Generate(this);
            Input[id] = message;
            return id;
        }

        public void Clear(StatusIdentifier identifier)
        {
            Input.Remove(identifier);
        }
    }
}
