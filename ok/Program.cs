using ok.cmd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ok
{
    enum Request
    {
        List,
        Update,
        Command
    }

    partial class Program
    {
        static readonly string DEFAULT_PLUGIN_GIST = "https://gist.githubusercontent.com/dedbeef/e02da7f6419608b00e6a6985e51d379d/raw";
        static readonly string[] BINARY_PATHS = {
            @"bin\x64\Debug",
            @"bin\x64\Release",
            @"bin\Debug",
            @"bin\Release",
        };
        static List<IOkPlugin> commands;
        static int alignTo = -1;

        static void Main(string[] args)
        {
            AddToPathEnvironmentVariable();
            LoadCommands();
            Request request = GetRequestFromArgs(args);
            IOkPlugin command = GetCommandFromArgs(args);
            string[] remainingArgs = args.Skip(1).ToArray();
            ProcessRequestAndCommand(request, command, remainingArgs);
        }

        /// <summary>
        /// Processes the given Request and IOkPlugin command
        /// </summary>
        /// <param name="request">The type of request</param>
        /// <param name="command">The IOkPlugin command (if applicable) or null</param>
        /// <param name="args">Any args for the command/request (if applicable)</param>
        static void ProcessRequestAndCommand(Request request, IOkPlugin command, string[] args)
        {
            switch (request)
            {
                case Request.List:
                    {
                        ListCommands();
                    }
                    break;
                case Request.Update:
                    {
                        //TODO: Work in progress
                        //Update();
                    }
                    break;
                case Request.Command:
                default:
                    {
                        if (command == null)
                        {
                            ListCommands();
                            return;
                        }
                        var dir = Directory.GetCurrentDirectory();
                        if (command.RunFromSourceDirectory)
                        {
                            Directory.SetCurrentDirectory(command.SourceDirectory);
                        }
                        var argsWereValid = command.Execute(args);
                        Directory.SetCurrentDirectory(dir);
                        if (!argsWereValid)
                        {
                            PrintCommand(command);
                        }
                    }
                    break;
            }

            if (command != null && command.PauseOnComplete)
                Console.ReadKey(true);
        }

        /// <summary>
        /// Updates the any repo matching the given git name.
        /// If the given name is null or empty, updates all repos
        /// </summary>
        /// <param name="gitNameMatching"></param>
        static void Update(string gitNameMatching = null)
        {
            /// TODO: finish implementation, check if git installed, warn untrusted plugins etc.
            throw new NotImplementedException();

            var url = GetReposUrl();
            var repos = new List<string>();
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    repos = client.DownloadString(url).Replace("\r", "").Split('\n').ToList();
                }
                if (repos.Count == 0)
                {
                    ConsoleColor.Red.WriteLine($"No repos found to update. Possible network or DNS error.");
                    return;
                }
            }
            catch (Exception ex)
            {
                ConsoleColor.Red.WriteLine($"Unable to check for updates:\n{ex}");
                return;
            }
            
            if (File.Exists("installed.txt"))
            {
                // Also check repos the user has installed
                repos.AddRange(File.ReadAllLines("installed.txt").Where(x => x.Trim().EndsWith(".git")));
            }
            // Find all repos that end with `{arg}.git`
            var matchingRepos = repos.Where(x => string.IsNullOrEmpty(gitNameMatching) || x.ToLower()
                                                        .Split('/').Last() == gitNameMatching.ToLower() + ".git")
                                                        .ToList();
            
            var updatedRepos = ClonePluginReposIfNewer(matchingRepos);
            ConsoleColor.Yellow.WriteLine($"Updated {updatedRepos.Count(x => x.Success)}/{updatedRepos.Count} plugins:");

            foreach (var (Url, Success) in updatedRepos)
            {
                var consoleColor = Success ? ConsoleColor.Gray : ConsoleColor.Red;
                consoleColor.WriteLine(Url);
            }
        }

        static List<(string Url, bool Success)> ClonePluginReposIfNewer(List<string> repos)
        {
            //TODO: finish implementation, check if git installed etc.
            throw new NotImplementedException();

            var result = new List<(string Url, bool Success)>();
            if (repos == null || repos.Count == 0)
                return result;

            return result;
        }


        /// <summary>
        /// Gets the URL for the repos used for updating or installing new plugins
        /// </summary>
        /// <returns>The URL from updates.txt or the default plugin gist URL</returns>
        static string GetReposUrl()
        {
            if (!File.Exists("updates.txt"))
            {
                File.WriteAllText("updates.txt", DEFAULT_PLUGIN_GIST);
                return DEFAULT_PLUGIN_GIST;
            }
            else
            {
                return File.ReadAllText("updates.txt").Trim();
            }
        }

        /// <summary>
        /// Parses the given args into a Request for processing
        /// </summary>
        /// <param name="args">The args passed into `ok`</param>
        /// <returns>A Request parsed from the first argument, otherwise Request.Command</returns>
        static Request GetRequestFromArgs(string[] args)
        {
            if(args == null)
            {
                return Request.Command;
            }
            if (args.Length > 0 && Enum.TryParse(args[0], out Request parsedRequest))
            {
                return parsedRequest;
            }
            return Request.Command;
        }

        /// <summary>
        /// Finds a loaded plugin command from the given args
        /// </summary>
        /// <param name="args">The args passed into `ok`</param>
        /// <returns>An IOkPlugin command from the loaded commands matching the arguments, otherwise null</returns>
        static IOkPlugin GetCommandFromArgs(string[] args)
        {
            return commands.FirstOrDefault(x => x.Command.ToLower() == args[0].ToLower());
        }

        /// <summary>
        /// Adds the path of `ok.exe` to the PATH variable in User Environment Variables.
        /// This allows the user to call it from powershell or command prompt easily.
        /// </summary>
        static void AddToPathEnvironmentVariable()
        {
            var thisPath = Path.GetDirectoryName(Path.GetFullPath("ok.exe"));
            var envPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            if (!envPath.Contains(thisPath))
            {
                Environment.SetEnvironmentVariable("Path", envPath + $";{thisPath}", EnvironmentVariableTarget.User);
                ConsoleColor.DarkMagenta.WriteLine("Installed `ok`, next time you can run with `ok` from CMD or Powershell.");
                Console.ReadKey(true);
                return;
            }
        }

        /// <summary>
        /// List all commands that have loaded
        /// </summary>
        static void ListCommands()
        {
            ConsoleColor.DarkMagenta.WriteLine("Commands:");
            if(commands.Count == 0)
            {
                ConsoleColor.White.WriteLine("No solution folders found with class libraries inheriting the IOkPlugin interface in:");
                ConsoleColor.DarkGray.WriteLine($"{GetSolutionPath().Parent.FullName}");
                return;
            }
            
            foreach(var command in commands)
            {
                PrintCommand(command);
            }
        }

        /// <summary>
        /// Prints the command name, the syntax and the description
        /// </summary>
        /// <param name="command"></param>
        static void PrintCommand(IOkPlugin command)
        {
            if (alignTo == -1)
                alignTo = command.Command.Length;
            ConsoleColor.White.Write($"{command.Command.ToLower().PadRight(alignTo + 4)}");
            ConsoleColor.Gray.Write($"{command.Syntax}\n");
            ConsoleColor.DarkGray.WriteLine(command.Description);
        }

        /// <summary>
        /// Loads all commands into memory from this projects solutions parent folder.
        /// Usually this will be looking in your default repos folder.
        /// </summary>
        static void LoadCommands()
        {
            var start = DateTime.Now;
            commands = new List<IOkPlugin>();
            var reposFolder = GetReposPath();
            foreach (var folder in reposFolder.GetDirectories())
            {
                var command = LoadPluginFromSolutionFolder(folder);
                if(command != null)
                {
                    commands.Add(command);
                }
            }
            if (commands.Count > 0)
            {
                commands.OrderByDescending(x => x.Command.Length);
                alignTo = commands[0].Command.Length;
            }
            var time = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine($"Loaded in {time:F2} seconds");
        }

        /// <summary>
        /// Gets the solution path for this project.
        /// Iterates through the parents until it finds a .sln file or the parent is null
        /// </summary>
        /// <param name="currentPath"></param>
        /// <returns>The first path containing a .sln file or null</returns>
        static DirectoryInfo GetSolutionPath()
        {
            var directory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            while (directory != null && directory.GetFiles("*.sln").Length == 0)
            {
                directory = directory.Parent;
            }
            return directory;
        }

        /// <summary>
        /// Attempts to find the users preferred repo path by looking at the parent of `ok`s solution path
        /// </summary>
        /// <returns>The repo path, otherwise null</returns>
        static DirectoryInfo GetReposPath()
        {
            var solutionFolder = GetSolutionPath();
            if (solutionFolder == null)
            {
                ConsoleColor.Red.WriteLine("Cannot find solution path for `ok`.\n" +
                    "`ok` is intended to be cloned into the default repo directory and run from there.\n" +
                    "Please see more instructions in the readme on the git repo.");
                return null;
            }
            var reposFolder = GetSolutionPath().Parent;
            if (reposFolder == null)
            {
                ConsoleColor.Red.WriteLine("Cannot find repos path.\n" +
                    "`ok` is intended to be cloned into the default repo directory and run from there.\n" +
                    "Please see more instructions in the readme on the git repo.");
                return null;
            }
            return reposFolder;
        }

        /// <summary>
        /// Attempts to load a plugin command from a solution folder if it contains one.
        /// If the solution file is found, the common build paths will be searched for the binary to load.
        /// If multiple builds exist, it will load the first found with highest version.
        /// </summary>
        /// <param name="pluginSolutionFolder">A folder ideally containing a solution for the plugin to load</param>
        /// <returns>The loaded plugin, otherwise null.</returns>
        static IOkPlugin LoadPluginFromSolutionFolder(DirectoryInfo pluginSolutionFolder)
        {
            IOkPlugin loadedPlugin = null;
            var highestVersionFound = 0.0;

            // We only want folders with a solution file
            if (pluginSolutionFolder.GetFiles().FirstOrDefault(x => x.Extension.ToLower() == ".sln") == null)
                return null;

            foreach (var subfolder in pluginSolutionFolder.GetDirectories())
            {
                foreach (var binaryPath in BINARY_PATHS)
                {
                    var fullBinaryPath = Path.Combine(subfolder.FullName, binaryPath);
                    if (Directory.Exists(fullBinaryPath))
                    {
                        if (!File.Exists(Path.Combine(fullBinaryPath, "ok.cmd.dll")))
                            continue;

                        var binFiles = new DirectoryInfo(fullBinaryPath).GetFiles("*.dll", SearchOption.TopDirectoryOnly);
                        foreach (var pluginBinary in binFiles)
                        {
                            loadedPlugin = LoadPluginIfNewer(pluginBinary, ref highestVersionFound);
                        }
                    }
                }
            }
            return loadedPlugin;
        }

        /// <summary>
        /// Attempts to load the given plugin command file if the version is higher than the given highest version found.
        /// Logs any failed plugin loads to `error.txt`.
        /// </summary>
        /// <param name="pluginBinary">The binary file of the plugin command</param>
        /// <param name="highestVersionFound">The highest version found so far for this plugin command</param>
        /// <returns>The loaded plugin, otherwise null</returns>
        static IOkPlugin LoadPluginIfNewer(FileInfo pluginBinary, ref double highestVersionFound)
        {
            try
            {
                var asm = Assembly.LoadFile(pluginBinary.FullName);
                IOkPlugin plugin = null;
                if (asm != null)
                {
                    foreach (var type in asm.GetTypes())
                    {
                        if (typeof(IOkPlugin).IsAssignableFrom(type))
                        {
                            plugin = (IOkPlugin)asm.CreateInstance(type.FullName);
                            plugin.GetType().GetProperty("SourceDirectory", BindingFlags.Public | BindingFlags.Instance).SetValue(plugin, pluginBinary.Directory);
                            if (plugin.Version > highestVersionFound)
                            {
                                highestVersionFound = plugin.Version;
                                return plugin;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("error.log", $"Plugin failed to load with:\n{ex}\n\n");
            }
            return null;
        }
    }
}
