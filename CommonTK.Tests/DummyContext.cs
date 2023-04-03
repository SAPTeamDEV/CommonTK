using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAPTeam.CommonTK;

namespace CommonTK.Tests
{
    internal class DummyContext : Context
    {
        public bool IsTest { get; set; }
        public InteractInterface PreStat { get; set; }

        public DummyContext(): base()
        {
            
        }

        public DummyContext(bool isTest = false): base(isTest)
        {
            
        }

        protected override void ArgsHandler(dynamic[] args)
        {
            IsTest = args[0];
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
