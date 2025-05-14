using SAPTeam.CommonTK.ExecutionPolicy;

namespace SAPTeam.CommonTK.Tests;

internal class DummyContext2 : Context
{
    private bool created;
    private readonly bool protectedTest;
    private readonly bool throwOnInitializer;
    private readonly bool throwOnRegisterAction;
    private readonly bool earlyRegisterAction;
    private readonly bool throwOnFinalizer;

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

        created = created ? throw new InvalidDataException() : true;

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
