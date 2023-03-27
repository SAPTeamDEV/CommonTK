# CommonTK - All in One and Multi Purpose .NET Library
[![Nuget](https://img.shields.io/nuget/v/SAPTeam.CommonTK?label=CommonTK)](https://www.nuget.org/packages/SAPTeam.CommonTK)
[![Nuget](https://img.shields.io/nuget/v/SAPTeam.CommonTK.Console?label=CommonTK.Console)](https://www.nuget.org/packages/SAPTeam.CommonTK.Console)
![Nuget](https://img.shields.io/nuget/dt/SAPTeam.CommonTK)

This library contains every things that you need for a Professional application.

## Installation
This project currently has two NuGet packages that you can add them with Package manager console.

SAPTeam.CommonTK
```
PM> Install-Package SAPTeam.CommonTK
```

SAPTeam.CommonTK.Console
```
PM> Install-Package SAPTeam.CommonTK.Console
```
CommonTK is dependency of Console library, so it will be installed automatically.

## CommonTK Features
This library has many Features and also provides Interfaces can be used in `Console` library and other .NET Application.

### Contexts
Contexts is a key feature in this library. With contexts you can set a global situation and this change is visible across the program using `Context.Current.HasContext<IContext>()`.

For using this feature you can implement a new class from `IContext` interface and `Context` base class, or use Contexts available in `Console` library.

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

## CommonTK.Console Features
This library has many utilities for doing Advanced action Directly with Console using main Win32 P/Invoke APIs.
and also has various amazing features for creating beautiful console User Interface.

### Console Form
Console form is the biggest feature in this library. With `ConsoleForm` you can create an amazing and different UI inside the Console window!

Users can control and select items with keyboard keys. You can Notify users with a `ScreenMessage` toast notification! Console Forms also support Multi paging without issues. It can manage and handle many pages using a platform named `Interface`.

#### Create a new `Form`
In the beginning you must create a `Form` for defining form Title, Options and Behaviors.
Here is a Example of simple Console Form:
```
public class ExampleForm : ConsoleForm.Form
{
    // Creates Form options. called just one time
    protected override void CreateItems()
    {
        // Create new Sections. if you dont need sections, use "" in section name.
        // Items[""] = new();

        // Sections ordered by name.
        Items["Tools"] = new();
        Items["Actions"] = new();

        // Adds Options to Specified sections.
        Items["Tools"].Add("Create a new File");
        Items["Tools"].Add("Open a File");
        Items["Tools"].Add("Open a Folder");

        Items["Actions"].Add("Close File");
        Items["Actions"].Add("Delete File");
        Items["Actions"].Add("Exit");
    }

    // Shows Title of Form. Called many times after each Refresh or during Form creation.
    protected override void OnTitle()
    {
        Utils.Echo("Example page");
        Utils.Echo("Press ESC to Close this page");

        // Because Console window is not notify program about Events, We have disabled the Close Button and users must enter ESC key to exit.

        // Write a Empty line for better look.
        Utils.Echo();
    }

    // Called just one time when Form is about to start and before reading user inputs.
    protected override void OnStart()
    {
        
    }

    // Called once before close and clearing the contents of form.
    protected override void OnClose()
    {
        
    }

    // This is the main logic Entry point of form. Called every time that a Option is selected by user.
    protected override void OnEnter(ConsoleOption option)
    {
        // This is the names that described in CreateItems().
        switch (option.Text)
        {
            // Handles functionality of option Create a new File.
            case "Create a new File":
                break;
            case "Delete File":
                // Creates and Takes Console control to the SubForm.
                Platform.AddSubForm(new DeleteFile());
                break;
            case "Exit":
                // If this form is a sub form, you must call CloseSubForm();
                Platform.Close();
                break;
            default:
                // notify user with a ScreenMessage.
                Platform.ScreenMessage("There is nothing");
                break;
        }
    }
}

// Create a SubForm.
// Sub Forms creation is same as Form but in initialization and closing is different.
public class DeleteFile : ConsoleForm.Form
{
    // Prohibit close this page with ESC key.
    public override bool IsClosable => false;

    // This Form is Always ceated as sub form.
    public DeleteFile() : base(false) { }

    // You can use all features described in Form. all this functions replaces Form functions until it closed.

    // Creates SubForm options. called during object ceation one time.
    protected override void CreateItems()
    {
        Items[""] = new() { "Yes", "No" };
    }

    protected override void OnTitle()
    {
        Utils.Echo("Delete this file?");
        Utils.Echo();
    }

    // Called when user select an option.
    protected override void OnEnter(ConsoleOption option)
    {
        // This is the names that described in CreateItems(). Yes or No.
        switch (option.Text)
        {
            case "Yes":
            case "No":
                // Return Control to the Form and Close this sub form.
                Platform.CloseSubForm();
                break;
        }
    }
}
```

#### Start Form
Console forms can be runned in any Console applications and even UI Applications using `ConsoleWindow` Context that described later.
For Console applications simply Create an Instance of your form and call `Start()`.

For Desktop applications you need to create a Console windows using `ConsoleWindow` Context:
```
using ConsoleWindow console = new();
var form = new ExampleForm();
form.Start();
```

### Contexts
This library has three Context classes that can be accessed trough `SAPTeam.CommonTK.Contexts`.

#### `ConsoleWindow`
This context is a key utility for desktop applications, with this Context you can Allocate a new Console window and simply close it.
```
using (ConsoleWindow console = new())
{
    // Write your code.
    Console.WriteLine("Hello");
}
```

#### `DisposableWriter`
This context gives you a temporary writing session that you can write your Contents in any color that you want and in end, Simply clear exactly what you wrote.

For using this Context you must use `Utils.Echo(string)` to correctly write your text and record it's coordinates in context.
```
using (DisposableWriter dw = new(backgroundColor: ConsoleColor.White, foregroundColor: ConsoleColor.Black))
{
    // Write your texts.
    Utils.Echo("Temporary text.");
}
```

#### `RedirectConsole`
Main usage of this Context is in Console Forms for hiding Form creation process.
```
using (RedirectConsole rc = new())
{
    // Run codes that have unwanted text writings.
}
```

### `Colorize`
With this struct you can format a text to have different colors in each parts of that.

Put every parts that you want to change it's color in a `[]` and determine colors in next parameters respectively. 
```
// Create a Colorized string.
Colorize colorizedString = new("Welcome to [Sample App] version [1.2]", ConsoleColor.Red, ConsoleColor.Green);

// Pass this string to Utils.Echo to process and write it.
Utils.Echo(colorizedString);

// In output text all words except "Sample App" and "1.2" Follows global color theme defined in ColorSet.Current.
// First word wrote with Red color and Second one is green.
```

### `ColorSet`
This is a Data-Type struct for keeping pairs of Foreground and Background colors, for example `ScreenMessage` uses ColorSet for setting it's color.
It also used for setting **Global Color Set**. This color is used in every methods that defined in this library.
```
// Creates a new Color Set with White background and Black text color.
ColorSet colors = new(ConsoleColor.White, ConsoleColor.Black);

// Changes current default Color Set. This also changes the System.Console color set.
ColorSet.Current = colors;

// Resets Global Color Set to the default Black/Gray Back/Fore.
ColorSet.Current = new();
```

### Console Manager
This is an Advanced utility intended for desktop Applications for get access to Console Windows using Win32 P/Invoke APIs.
A simple usage of methods that provided by `ConsoleManager` is available with `ConsoleWindow` Context. but this class give you more options for creating and releasing a Console.

There is Different ways to creating Console window:
- Allocation (Default): In this way, Library calls AllocConsole() Win32 API to Allocate a new Console window to this application.
- AttachToParent (Unreliable): In this way Application attaches to the Console that called this application.
- AttachProcess: Starts a new cmd.exe process, then kill it and uses Console window provided with that process.

There is also some Limitations:
- Applications can't have more than One Console windows.
- Closing console window also Kills application process Immediately.

For resolving second issue, we have Disabled the Close button of console window. Applications must implement a way for Calling `ConsoleManager.HideConsole()` after job is finished, otherwise Console is stay unresponsive. When using `ConsoleWindow` Context this method called automatically.

### Utils

#### `Echo`
This method is the main handler of all text-related actions in this library. if you're using COntexts of Colorized Strings you must use this method for writing texts.

#### `ClearLine`
Clears Previous or Current line contents.

#### `SetColor`
Temporarily changes color of System.Console, but don't change The **Global Color Set** and simply can be reverted with `ResetColor()`.
Don't confused with `System.Console.ResetColor` it's don't take care of Global Color Set.

## Contribution
Feel free to grab the source, open issues or pull requests.