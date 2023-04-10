using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPTeam.CommonTK.Tests
{
    internal class ProtectedExposerContext : Context
    {
        public override string[] Groups => new string[]
        {
            "application.test4"
        };

        public override string[] NeutralGroups => new string[]
        {
            "application.test",
            "application.test5"
        };

        public ProtectedExposerContext()
        {
            Initialize(true);
        }

        protected override void CreateContext()
        {
            
        }

        protected override void DisposeContext()
        {
            
        }

        public void Suppress(string hroup) => SuppressLock(hroup);
        public void Lock(string group) => LockGroup(group);
    }
}
