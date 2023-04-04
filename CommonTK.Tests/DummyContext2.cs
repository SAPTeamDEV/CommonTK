using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAPTeam.CommonTK;

namespace SAPTeam.CommonTK.Tests
{
    internal class DummyContext2 : Context
    {
        bool created;

        public DummyContext2() : this(false)
        {

        }

        public DummyContext2(bool isGlobal = true)
        {
            Initialize(isGlobal);
        }

        protected override void CreateContext()
        {
            if (created)
            {
                throw new InvalidDataException();
            }
            else
            {
                created = true;
            }
        }

        protected override void DisposeContext()
        {

        }
    }
}
