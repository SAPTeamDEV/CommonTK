using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    internal class DummyContext3 : Context
    {
        public override string[] Groups => new string[] {"application.test"};

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
