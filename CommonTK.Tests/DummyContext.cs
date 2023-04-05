using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAPTeam.CommonTK;

namespace SAPTeam.CommonTK.Tests
{
    internal class DummyContext : Context
    {
        public bool IsTest { get; set; }
        public InteractInterface PreStat { get; set; }

        public override string[] Groups => new string[] { "global.interface" };

        public DummyContext() : this(false)
        {

        }
        public DummyContext(bool isTest = false, bool isGlobal = true)
        {
            IsTest = isTest;
            Initialize(isGlobal);
        }

        protected override void CreateContext()
        {
            PreStat = Interface;
            Interface = InteractInterface.None;
        }

        protected override void DisposeContext()
        {
            Interface = PreStat;
        }
    }
}
