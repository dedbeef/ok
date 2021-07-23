using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ok.cmd
{
    //public abstract class CmdBase
    //{
    //    /// <summary>
    //    /// The command name, this is not case-sensitive and cannot have spaces
    //    /// </summary>
    //    public virtual string Command => null;
    //    /// <summary>
    //    /// A short description of the command, limited to 100 characters.
    //    /// Any extra characters will be truncated.
    //    /// </summary>
    //    public virtual string Description => null;
    //    /// <summary>
    //    /// The syntax for the command e.g.
    //    /// exampleCmd firstArg secondArg (optionalThirdArg) (optionalFourthArg)
    //    /// </summary>
    //    public virtual string Syntax => null;
    //    /// <summary>
    //    /// The version number, used to determine the best binary to load for this command.
    //    /// </summary>
    //    public virtual double Version => 0.0;
    //    /// <summary>
    //    /// Whether the console should pause at the end or just close.
    //    /// </summary>
    //    public virtual bool PauseOnComplete => false;
    //    /// <summary>
    //    /// If true, runs from the source directory rather than running from the current directory.
    //    /// </summary>
    //    public virtual bool RunFromSourceDirectory => false;
    //    /// <summary>
    //    /// The directory of the plugin .DLL, this is automatically assigned before the plugin is executed.
    //    /// Useful for referencing files relative to the .DLL location.
    //    /// </summary>
    //    public string SourceDirectory { get; set; }

    //    /// <summary>
    //    /// Executes the command (place your code here) and returns true if the arguments were valid.
    //    /// </summary>
    //    /// <param name="args">The arguments for the command</param>
    //    /// <returns>true if the args were valid for the command, otherwise false.</returns>
    //    public virtual bool Execute(params string[] args)
    //    {
    //        return false;
    //    }
    //}

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
