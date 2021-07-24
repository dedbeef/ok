# ok
A simple plugin supported CLI tool.

#### Requirements
- .NET 4.8
- Windows 7/10/11
- Some way to clone the repo, e.g. `git` or `Visual Studio`.

#### Terms
**default repo folder** - Usually `C:\Users\Username\source\repos\`

## Installation
1. Clone (or fork then clone) this repo using Visual Studio to the default repo folder.
   - `git clone https://github.com/dedbeef/ok.git`
2. Build or run the included build once.
   - Located in `bin\Debug\ok.exe`

## Installing commands
1. Clone any command plugin repo to the default repo folder.
2. Build it if it does not already have binaries included.
   - `ok` will automatically find these cloned repos and the binaries to use

## Usage
1. Open a command prompt or powershell window anywhere.
2. Type `ok` to see all installed commands (see **Installing commands** above)
3. Run any command with the syntax that is shown beside each command.
   - e.g. `ok code.cs edit BinDiff`

## Making new commands
1. **Create your project:**
   - Open Visual Studio and create a new `C# Class Library (.NET Framework)` project
   - Note the name of the solution, project and even the class do **not** matter.
   - I recommend renaming the ugly `Class1.cs` to something like `Plugin.cs` though
   
2. **Add a reference to `ok.cmd.dll`:**
   - In the Solution Explorer, right click `References` and select `Add Reference...`
   - Select the `Browse` tab, then click `Browse...`
   - Navigate to `ok\ok.cmd\bin\Debug\ok.cmd.dll` and click OK, OK
   
3. **Implement the plugin interface:**
   - e.g. `public class MyClass : ok.cmd.IOkPlugin`
   - (Optional) Use Visual Studio intellisense quick fix to implement the missing members
   ```cs
   // Example plugin
   public class Plugin : ok.cmd.IOkPlugin
   {
        public string Command => "mycmd";
        public string Description => "An example description of your command";
        public string Syntax => "mycmd someArg (someOptionalArg)";
        public double Version => 1.0;

        public bool PauseOnComplete => false;
        public bool RunFromSourceDirectory => false;
        public string SourceDirectory { get; set; }

        public bool Execute(params string[] args)
        {
            Console.WriteLine("Hello World");
            return true;
        }
   }
   ```
