using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ok.cmd
{
    public interface IOkPlugin
    {
        /// <summary>
        /// The command name, this is not case-sensitive and cannot have spaces
        /// </summary>
        string Command { get; }
        /// <summary>
        /// A short description of the command, limited to 100 characters.
        /// Any extra characters will be truncated.
        /// </summary>
        string Description { get; }
        /// <summary>
        /// The syntax for the command e.g.
        /// exampleCmd firstArg secondArg (optionalThirdArg) (optionalFourthArg)
        /// </summary>
        string Syntax { get; }
        /// <summary>
        /// The version number, used to determine the best binary to load for this command.
        /// </summary>
        double Version { get; }
        /// <summary>
        /// Whether the console should pause at the end or just close.
        /// </summary>
        bool PauseOnComplete { get; }
        /// <summary>
        /// If true, runs from the source directory rather than running from the current directory.
        /// </summary>
        bool RunFromSourceDirectory { get; }
        /// <summary>
        /// The directory of the plugin .DLL, this is automatically assigned before the plugin is executed.
        /// Useful for referencing files relative to the .DLL location.
        /// </summary>
        string SourceDirectory { get; set; }

        /// <summary>
        /// Executes the command (place your code here) and returns true if the arguments were valid.
        /// </summary>
        /// <param name="args">The arguments for the command</param>
        /// <returns>true if the args were valid for the command, otherwise false.</returns>
        bool Execute(params string[] args);
    }
}
