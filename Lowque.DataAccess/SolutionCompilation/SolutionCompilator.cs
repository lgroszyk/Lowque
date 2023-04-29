using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace Lowque.DataAccess.SolutionCompilation
{
    public class SolutionCompilator : ISolutionCompilator
    {
        private readonly IConfiguration appConfiguration;
        private SolutionOptions options;

        public SolutionCompilator(IConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.options = SolutionOptions.Default;
        }

        public CompilationResult Compile(string appName, SolutionOptions options)
        {
            this.options = options;
            return Compile(appName);
        }

        public CompilationResult Compile(string appName)
        {
            var workspace = appConfiguration["Configuration:Workspace"];
            var toBeBuiltPath = Path.Combine(workspace, GetSolutionFolder(), appName);
            var logFilePath = Path.Combine(workspace, "Temporary", $"{appName}.log");

            var compileProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = appConfiguration["Configuration:DotnetCli"],
                    Arguments = $"{GetCompileCommand()} {toBeBuiltPath} {GetCompileCommandConfigurationParameter()} /flp:v=q;logfile={logFilePath}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            compileProcess.Start();
            compileProcess.WaitForExit();

            var logs = File.ReadAllLines(logFilePath);
            return new CompilationResult
            {
                Logs = logs
            };
        }

        private string GetSolutionFolder()
        {
            switch (options.Type)
            {
                case SolutionType.Application: return "Apps";
                case SolutionType.Temporary: return "Temporary";
                default: throw new NotSupportedException($"The {options.Type} type of SolutionCompilator is not supported.");
            }
        }

        private string GetCompileCommand()
        {
            switch (options.Type)
            {
                case SolutionType.Application: return "publish";
                case SolutionType.Temporary: return "build";
                default: throw new NotSupportedException($"The {options.Type} type of SolutionCompilator is not supported.");
            }
        }

        private string GetCompileCommandConfigurationParameter()
        {
            switch (options.Type)
            {
                case SolutionType.Application: return "-c Release";
                case SolutionType.Temporary: return "";
                default: throw new NotSupportedException($"The {options.Type} type of SolutionCompilator is not supported.");
            }

        }
    }
}
