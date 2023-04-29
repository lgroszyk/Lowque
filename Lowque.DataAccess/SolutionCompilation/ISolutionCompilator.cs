namespace Lowque.DataAccess.SolutionCompilation
{
    public interface ISolutionCompilator
    {
        CompilationResult Compile(string appName);
        CompilationResult Compile(string appName, SolutionOptions options);
    }
}
