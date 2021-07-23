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
    class Program
    {
        static readonly string[] possibleBinaryPaths = {
            @"bin\x64\Debug",
            @"bin\x64\Release",
            @"bin\Debug",
            @"bin\Release",
        };
        static List<IOkPlugin> commands;

        static void Main(string[] args)
        {
            var thisPath = Path.GetDirectoryName(Path.GetFullPath("ok.exe"));
            var envPath = Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User);
            if (!envPath.Contains(thisPath))
            {
                Environment.SetEnvironmentVariable("Path", envPath + $";{thisPath}", EnvironmentVariableTarget.User);
                Console.WriteLine("Installed `ok`, next time you can run with `ok` from CMD or Powershell.");
                Console.ReadKey(true);
                return;
            }
            LoadCommands();
            IOkPlugin command = null;
            if(args.Length == 0 || args[0].ToLower() == "list")
            {
                ListCommands();
            }
            else
            {
                command = commands.FirstOrDefault(x => x.Command.ToLower() == args[0].ToLower());
                if(command == null)
                {
                    ListCommands();
                }
                else
                {
                    var dir = Directory.GetCurrentDirectory();
                    if(command.RunFromSourceDirectory)
                    {
                        Directory.SetCurrentDirectory(command.SourceDirectory);
                    }
                    var validArgs = command.Execute(args.Skip(1).ToArray());
                    Directory.SetCurrentDirectory(dir);
                    if(!validArgs)
                    {
                        PrintSyntax(command);
                    }
                }
            }
            if(command != null && command.PauseOnComplete)
                Console.ReadKey(true);
        }

        static void ListCommands()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("Commands:");
            if(commands.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("No solution folders found with class libraries inheriting from CmdBase in:");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"{GetSolutionPath().Parent.FullName}");
                Console.ResetColor();
                return;
            }
            commands.OrderByDescending(x => x.Command.Length);
            var longest = commands[0].Command.Length;
            foreach(var command in commands)
            {
                PrintSyntax(command, longest);
            }
            Console.ResetColor();
        }

        static void PrintSyntax(IOkPlugin command, int alignTo = -1)
        {
            if(alignTo == -1)
                alignTo = command.Command.Length;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{command.Command.ToLower().PadRight(alignTo + 4)}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"{command.Syntax}\n");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(command.Description);
            Console.ResetColor();
        }

        static void LoadCommands()
        {
            var start = DateTime.Now;
            commands = new List<IOkPlugin>();
            var reposFolder = GetSolutionPath().Parent;
            foreach (var folder in reposFolder.GetDirectories())
            {
                IOkPlugin current = null;
                var latestVersion = 0.0;

                if (folder.GetFiles().FirstOrDefault(x => x.Extension.ToLower() == ".sln") == null)
                    continue;

                foreach (var subfolder in folder.GetDirectories())
                {
                    foreach (var possibleBinPath in possibleBinaryPaths)
                    {
                        var binPath = Path.Combine(subfolder.FullName, possibleBinPath);
                        if (Directory.Exists(binPath))
                        {
                            if (!File.Exists(Path.Combine(binPath, "ok.cmd.dll")))
                                continue;

                            var binFiles = new DirectoryInfo(binPath).GetFiles("*.dll", SearchOption.TopDirectoryOnly);
                            foreach (var binFile in binFiles)
                            {
                                try
                                {
                                    var asm = Assembly.LoadFile(binFile.FullName);
                                    IOkPlugin plugin = null;
                                    if (asm != null)
                                    {
                                        foreach (var type in asm.GetTypes())
                                        {
                                            if (typeof(IOkPlugin).IsAssignableFrom(type))
                                            {
                                                plugin = (IOkPlugin)asm.CreateInstance(type.FullName);
                                                plugin.GetType().GetProperty("SourceDirectory", BindingFlags.Public | BindingFlags.Instance).SetValue(plugin, binPath);
                                                if (plugin.Version > latestVersion)
                                                {
                                                    current = plugin;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
                if (current != null)
                {
                    commands.Add(current);
                }
            }
            var time = (DateTime.Now - start).TotalSeconds;
            Console.WriteLine($"Loaded in {time:F2} seconds");
        }

        static DirectoryInfo GetSolutionPath(string currentPath = null)
        {
            var directory = new DirectoryInfo(
                currentPath ?? Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            while (directory != null && !directory.GetFiles("*.sln").Any())
            {
                directory = directory.Parent;
            }
            return directory;
        }
    }
}
