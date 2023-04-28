using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    internal class DummyContext3 : Context
    {
        public override string[] Groups { get; } = new string[]
        {
            "application.test",
            "application.test5"
        };

        public DummyContext3()
        {
            Initialize(true);
        }

        protected override void CreateContext()
        {
            
        }

        protected override void DisposeContext()
        {
            
        }
    }
}
