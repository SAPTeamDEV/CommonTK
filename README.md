# CommonTK - All-in-One and Multi-Purpose .NET Library

[![Gawe CI](https://github.com/SAPTeamDEV/CommonTK/actions/workflows/main.yml/badge.svg?event=push)](https://github.com/SAPTeamDEV/CommonTK/actions/workflows/main.yml)
[![codecov](https://codecov.io/gh/SAPTeamDEV/CommonTK/graph/badge.svg?token=OT7SS9OCPT)](https://codecov.io/gh/SAPTeamDEV/CommonTK)
[![NuGet](https://img.shields.io/nuget/v/SAPTeam.CommonTK)](https://www.nuget.org/packages/SAPTeam.CommonTK)
[![NuGet](https://img.shields.io/nuget/dt/SAPTeam.CommonTK)](https://www.nuget.org/packages/SAPTeam.CommonTK)

CommonTK is a feature-rich toolkit for .NET applications, providing advanced context management, status providers, hierarchical settings, action groups, and more. It is designed for flexibility, extensibility, and performance.

## Installation

Install with dotnet CLI:
```bash
dotnet add package SAPTeam.CommonTK
```

## Features

### Contexts

Contexts allow you to define and manage global or private application states, with support for locking execution via action groups.

**Example: Custom Context**


```
public class ExampleContext : Context
{
    public override string[] Groups => new[] { Context.ActionGroup(ActionScope.Application, "sample_action") };

    public ExampleContext()
    {
        Initialize(true); // Register as global context. must be called after ctor logic, if not called, the context won't be registered and not working.
    }

    protected override void CreateContext()
    {
        // Initialization logic here
    }

    protected override void DisposeContext()
    {
        // Cleanup logic here
    }
}

```

**Usage:**


```
using (var context = new ExampleContext())
{
    // Code here runs with the context changes.
    // In this state, the action group application.sample_action is locked and all methods that use it will be blocked.
}

// Action group is unlocked here.
```

### Action Groups

Action groups let you prevent unintended changes that may conflict eith the running context.

```
public void ModifyUnintendedValues()
{
    Context.QueryGroup(Context.ActionGroup(ActionScope.Application, "sample_action"));
    // Code here will only run if the action group is not locked
}
```

### Hierarchical Settings

Define and manage settings in a hierarchical structure, with type convention and import/export capabilities.


```
var root = new SettingsStore();
var setting = root.CreateSetting("app.theme", "dark", "UI theme");
setting.Value = "light";
string theme = setting.Value;

```

### Status Providers

Unified interfaces for reporting status, progress, and multi-status bars.

**Basic Status Provider:**

```
public class UIStatusProvider : IStatusProvider
{
    private readonly Label status;

    public UIStatusProvider(Label label) => status = label;

    public void Clear() => status.Text = "";

    public StatusIdentifier Write(string message)
    {
        status.Text = message;
        return StatusIdentifier.Empty;
    }
}
```

### Registry

A simple registry for managing resources with resource locations.

```
var registry = new Registry<Stream>();

// Register a resource
var location = new ResourceLocation("private", "myimage");
var stream = File.OpenRead("path/to/image.png");
registry.TryAdd(location, stream);

// Retrieve a resource
var file = registry[location];
```

### Timer

Call a method after a specified time interval, with optional repeat functionality and exception handling.


```
var timer = new Timer(5000, () => SendMessage("test"), repeat: false);

// Stop or pause before the timer ends
timer.Pause(); // Only for repeating timers
timer.Stop();

// Get thrown exceptions
var exception = timer.Exceptions.FirstOrDefault();
```

## Security Reporting

If you discover any security vulnerabilities, please report them by following our [Security Guidelines](https://github.com/SAPTeamDEV/CommonTK/blob/master/SECURITY.md).

## Contributing

We welcome contributions! Please see our [Contributing guide](https://github.com/SAPTeamDEV/CommonTK/blob/master/CONTRIBUTING.md) for more information on how to get started.

## License

This project is licensed under the [MIT License](https://github.com/SAPTeamDEV/CommonTK/blob/master/LICENSE.md).
