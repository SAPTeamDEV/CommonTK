using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    public class DummyStatusProvider : IStatusProvider
    {
        public StringBuilder Input { get; }

        public DummyStatusProvider()
        {
            Input = new StringBuilder();
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
            Input.Append(message);
            return StatusIdentifier.Empty;
        }
    }
}
