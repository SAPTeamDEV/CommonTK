# CommonTK.Console - All in One and Multi Purpose .NET Library for Professional Console actions
This library has many utilities for doing Advanced action Directly with Console using main Win32 P/Invoke APIs.
and also has various amazing features for creating beautiful console User Interface.

## Installation
You can install this library with Package manager console.

SAPTeam.CommonTK.Console
```
PM> Install-Package SAPTeam.CommonTK.Console
```
CommonTK is dependency of Console library, so it will be installed automatically.

## Features

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