# CommonTK - All in One and Multi Purpose .NET Library
This library has many Features and also provides Interfaces can be used in `CommonTK.Console` library and other .NET Application.

## Installation
You can install this library with Package manager console.

SAPTeam.CommonTK
```
PM> Install-Package SAPTeam.CommonTK
```

## Features

### Contexts
Contexts is a key feature in this library. With contexts you can set a global situation and this change is visible across the program using `Context.Current.HasContext<IContext>()`.

For using this feature you can implement a new class from `IContext` interface and `Context` base class, or use Contexts available in `CommonTK.Console` library.

### `JsonWorker` and `Config`
`JsonWorker` is a base class for doing Json-related actions. A best place to use Json file, is Application onfig.
`Config` class do it simply Just by getting a file name and a `Serializer` class.

This class gives you config data in `Config.Prefs` property that you can make changes on it and save changes using `Config.Write()` method.
This is the Simplest way to save your Application settings or other type of data!

```
public class Entries
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

// Loads config.json, if it is not existing create it.
Config<Entries> config = new("config.json");

// Set new values.
config.Prefs.UserName = "admin";
config.Prefs.Password = "12345";

// Save config.json.
config.Write();
```

### Status Providers
There is a set of Interfaces and methods to work with `IStatusProvider` classes.
This feature intended for Interacting with users and must be implemented by Application for work in a specified way.

You can deal with statuses using static methods of `Interact` class or directly with `StatusProvider.Current`.
First you must Create and Assign a new instance of a class that implements `IStatusProvider` using `Interact.AssignStatus(IStatusProvider)` or:
```
StatusProvider.Current = new StatusProvider();
```

There is a variety types of Status Providers. All of that implements `IStatusProvider` as root Interface.

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
This interface is intended to use when you need to show a message with progress bar.
It has two method `Write(string, ProgressType)` and `Increment(int)` for writing a message with progress bar and incrementing value of progress bar respectively.
also Classes that implement this interface can throw exception `Write(string)` is called.

#### `IMultiStatusBar`
This interface intended for complicated usages. It is useful when you deal with a `Control` that support item collections, such as `StatusStrip`.
you must declare a method to manage and control multi item collections.

This is a code from my [AndroCtrl](https://github.com/SAPTeamDEV/AndroCtrl) project:
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
Almost all the Classes of this library were extracted from The public API of my private repository `Windows Pro` and published in two libraries, [CommonTK](https://github.com/SAPTeamDEV/CommonTK) and [CommonTK.Console](https://github.com/SAPTeamDEV/CommonTK.Console)