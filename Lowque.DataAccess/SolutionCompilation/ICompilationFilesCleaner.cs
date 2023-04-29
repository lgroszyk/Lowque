namespace Lowque.DataAccess.SolutionCompilation
{
    public interface ICompilationFilesCleaner
    {
        void CleanCompilationFiles(string appName);
        void CleanCompilationFiles(string appName, SolutionOptions options);
    }
}
