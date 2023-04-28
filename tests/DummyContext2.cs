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
        bool protectedTest;
        bool throwOnInitializer;
        bool throwOnRegisterAction;
        bool earlyRegisterAction;
        bool throwOnFinalizer;

        public DummyContext2() : this(false)
        {

        }

        public DummyContext2(bool isGlobal = true,
                             bool protectedTest = false,
                             bool throwOnInitializer = false,
                             bool throwOnRegisterAction = false,
                             bool earlyRegisterAction = false,
                             bool throwOnFinalizer = false)
        {
            this.protectedTest = protectedTest;
            this.throwOnInitializer = throwOnInitializer;
            this.throwOnRegisterAction = throwOnRegisterAction;
            this.earlyRegisterAction = earlyRegisterAction;
            this.throwOnFinalizer = throwOnFinalizer;

            Initialize(isGlobal);
        }

        public override string[] Groups { get; } = new string[] { "global.interface", ActionGroup(ActionScope.Application, "test") };
        public override string[] NeutralGroups { get; } = new string[]
        {
            ActionGroup(ActionScope.Application, "test2")
        };

        protected override void CreateContext()
        {
            if (throwOnInitializer)
            {
                throw new Exception();
            }

            if (throwOnRegisterAction)
            {
                SuppressLock("application.test");
            }

            if (earlyRegisterAction)
            {
                LockGroup("application.test");
            }

            if (created)
            {
                throw new InvalidDataException();
            }
            else
            {
                created = true;
            }

            if (IsGlobal && protectedTest)
            {
                SuppressLock("application.test");
                LockGroup("application.test2");
            }
        }

        protected override void DisposeContext()
        {
            if (throwOnFinalizer)
            {
                throw new Exception();
            }
        }
    }
}
