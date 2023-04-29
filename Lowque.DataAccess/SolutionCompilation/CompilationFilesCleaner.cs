using Lowque.DataAccess.Utils;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Lowque.DataAccess.SolutionCompilation
{
    public class CompilationFilesCleaner : ICompilationFilesCleaner
    {
        private readonly IConfiguration appConfiguration;
        private SolutionOptions options;

        public CompilationFilesCleaner(IConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.options = SolutionOptions.Default;
        }

        public void CleanCompilationFiles(string appName)
        {
            var workspace = appConfiguration["Configuration:Workspace"];
            var temporaryFilesPath = Path.Combine(workspace, "Temporary");
            IoExtensions.ClearDirectory(temporaryFilesPath);
        }

        public void CleanCompilationFiles(string appName, SolutionOptions options)
        {
            this.options = options;
            CleanCompilationFiles(appName);
        }
    }
}
