# CommonTK - All in One and Multi Purpose .NET Library

[![Gawe CI](https://github.com/SAPTeamDEV/CommonTK/actions/workflows/main.yml/badge.svg?event=push)](https://github.com/SAPTeamDEV/CommonTK/actions/workflows/main.yml)
[![NuGet](https://img.shields.io/nuget/v/SAPTeam.CommonTK)](https://www.nuget.org/packages/SAPTeam.CommonTK)
[![NuGet](https://img.shields.io/nuget/dt/SAPTeam.CommonTK)](https://www.nuget.org/packages/SAPTeam.CommonTK)

This library has many Features and also provides Interfaces can be used in .NET Applications.
This library has rich Interfaces that can be implemented and used by .NET libraries to create amazing features.

## Installation
You can install this library with Package manager console.

SAPTeam.CommonTK
```
PM> Install-Package SAPTeam.CommonTK
```

## Whats's News in version 2!
The new version of CommonTK Arrived with rewriting the everything!
* Introduce the Action Group mechanism for control code execution process.
* Change the Context initialization mechanism and defining new variables.
* Add `Variable` class for interacting with environment variables.
* Add ability to create private contexts.
* Change the underlying storing contexts implementation for better performance.
* Add test suit for improving predictable behavior.
* Merge `Context` and `ContextContainer` for providing a simple managing interface.
* Integrate all functions of static `Interact` class to `StatusProvider` class.
* Removed unnecessary class inheritances.
* and many minor patches to the entire library.

## Features

### Contexts
Contexts is a key feature in this library. With contexts you can set a global situation and this change is visible to the entire process using `Context.Exists<Context>()`.
Also this class hosts some global variables like `Interface` as well as some features like Action Groups.

For using this feature you can implement a new class from `Context` abstract class, or use Contexts available in `CommonTK.Console` library.

Here is an example of implementing a new context with the new API:
```
public class ExampleContext : Context
{
    // Defines the context Action Groups.
	// Action Group names can be anything, but using the below method will generate an standardized string for defining an action group name.
    // A context can have multi action groups.
    // The action groups only applied in the global contexts.
    public override string[] Groups => new string[] { Context.ActionGroup(ActionScope.Application, "sample_action") };

    public ExampleContext()
    {
        // Initializes the context. MUST be called in the end of the constructor.
        // If the argument is false, the context is not registered in the global dictionary.
        // Private contexts can be mandatory registered as global by initializing it with:
        // Context.Register<ExampleContext>();
        // This method only works for parameterless contexts.
        Initialize(true);
    }

    protected override void CreateContext()
    {
        // Put your codes here...
        // After finishing this method, All specified action groups locked until running DisposeContext()
    }

    protected override void DisposeContext()
    {
        // In this stage, the action groups will be unlocked.
        // Put your dispose codes here...
    }
}
```

### Action Groups
In the new version, we have introduced a new feature called `Action Groups`.
With this feature you can control execution of specific parts of your codes by defining and locking one or more action groups when a context is being used.

In the above example, the ExampleContext has defined one Action Group specified with `Context.ActionGroup(ActionScope.Application, "sample_action")`.
We use this action group name for showing the usage of this feature.
```
// This method has a conflicting behavior with the ExampleContext.
// With action groups you can prevent the execution of this method and all methods that uses the same action group name.
// The action group name of this method is: application.sample_action
// You can mention the action group name of each method or properties in their xml documentation for easier usage.
public void ModifyUnintendedValues()
{
    // This method will check the passed action group name locking status.
    // If the action group name is locked, It throws an ActionGroupException and prevents the execution of the rest of the code.
    Context.QueryGroup(Context.ActionGroup(ActionScope.Application, "sample_action"));
    // Put your codes after this.
}

void Main()
{
    using (var context = new ExampleContext())
    {
        // Calling this method inside the context will cause it to throw an exception.
        ModifyUnintendedValues();
    }

    // Calling this method outside the context will run it normally.
    ModifyUnintendedValues();
}
```

### Variable
With this class you can change the environment variable vlaues easily.
Also because the instances of this class caches the environment values the overall performance of your applications will be increased.

### Status Providers
There is a set of Interfaces and methods to work with `IStatusProvider` classes.
This feature intended for Interacting with users and must be implemented by Application for work in a specified way.

You can deal with statuses using static methods of `StatusProvider` class or directly with method provided in each status instances.
First you must Create and Assign a new instance of a class that implements `IStatusProvider` using the global status provider:
```
StatusProvider.Provider = new ExampleStatusProvider();
```

Also you can create and use infinite local status provider by simply initializing a new instance!
It is not necessary to set a status provider in global scope, but if you want to use the status provider simply across the different parts of your application using static methods of `StatusProvider` class, you can use it.

There is a variety types of Status Providers. All of these types implements `IStatusProvider` as root Interface.

#### `IStatusProvider`
This interface has a basic functionality. Just a `Write(string)` and `Clear()` method for writing and clearing text.
It is suitable for simple uses such as using a single label as Status Provider.

```
public class UIStatusProvider : IStatusProvider
{
    private readonly Label status;

    public UIStatusProvider(Label label)
    {
        status = label;
    }

    public void Clear()
    {
        status.Text = "";
    }

    public void Write(string message)
    {
        status.Text = message;
    }
}
```

#### `IProgressStatusProvider`
This interface is intended to use when you need to show a message with a progress bar.
It has two method `Write(string, ProgressType)` and `Increment(int)` for writing a message with a progress bar and incrementing value of progress bar respectively.
also Classes that implements this interface can throw exception when the method `Write(string)` is called.

#### `IMultiStatusBar`
This interface intended for complicated usages. It is useful when you deal with a `Control` that support item collections, such as `StatusStrip`.
you must declare a method to manage and control multi item collections.

This is a code from my [AndroCtrl](https://github.com/SAPTeamDEV/AndroCtrl) project (Note: This class is not adopted with the new API):
```
public class AppStatusProvider : IProgressStatusProvider, IMultiStatusProvider
{
	readonly StatusStrip statusbar;
	ToolStripProgressBar progressbar;

	bool gc;

	readonly Dictionary<string, (ToolStripLabel label, ToolStripProgressBar progressBar)> packets = new();

	public AppStatusProvider(StatusStrip statusbar, bool garbageCollection = true)
	{
		this.statusbar = statusbar;
		gc = garbageCollection;
	}

	public void Clear()
	{
		if (statusbar.Items.Count > 0)
		{
			foreach (var packet in packets)
			{
				if (packet.Value.progressBar != progressbar)
				{
					Clear(packet.Key);
				}
			}

			if (progressbar != null)
			{
				throw new InvalidOperationException("Can't remove an unfinished progress bar.");
			}
		}
	}

	public void Clear(string message)
	{
		if (packets[message].progressBar == progressbar)
		{
			progressbar = null;
		}

		statusbar.Items.Remove(packets[message].label);
		statusbar.Items.Remove(packets[message].progressBar);
		packets.Remove(message);
	}

	public void Write(string message)
	{
		throw new NotImplementedException();
	}

	public void Write(string message, ProgressBarType type)
	{
		if (progressbar != null && type == ProgressBarType.Block)
		{
			throw new InvalidOperationException("Can't register more than one block progress bar.");
		}
		if (packets.ContainsKey(message))
		{
			throw new ArgumentException("Can't use a duplicated status message: ", message);
		}

		ToolStripLabel label = new(message);
		statusbar.Items.Add(label);

		switch (type)
		{
			case ProgressBarType.None:
				throw new ArgumentException("type can't be None.");
			case ProgressBarType.Wait:
				ToolStripProgressBar loadingbar = new();
				loadingbar.Style = ProgressBarStyle.Marquee;
				statusbar.Items.Add(loadingbar);
				packets[message] = (label, loadingbar);
				break;
			case ProgressBarType.Block:
				progressbar = new();
				statusbar.Items.Add(progressbar);
				packets[message] = (label, progressbar);
				break;
		}
	}

	public void Increment(int value)
	{
		if (value == -1)
		{
			progressbar.PerformStep();
		}
		else
		{
			progressbar.Increment(value);
		}

		if (gc && progressbar.Value >= 100)
		{
			Clear(packets.Where((x) => x.Value.progressBar == progressbar).First().Key);
		}
	}
}
```

### Timer
Starts a simple timer in separate thread and calls the `callback` once or several times.
```
var timer = CommonTK.Timer.Set(5000. () => Environment.Exit(0), repeat: false);
```

## Contribution
Feel free to grab the source, open issues or pull requests.

## Credits
Almost all the Classes of this library were extracted from The public API of my private repository `Windows Pro` and published in two libraries, [CommonTK](https://github.com/SAPTeamDEV/CommonTK) and [CommonTK.Console](https://github.com/SAPTeamDEV/CommonTK.Console).
All these classes were rewrote in the new version.
