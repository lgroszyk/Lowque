namespace Lowque.DataAccess.SolutionCompilation
{
    public class SolutionOptions
    {
        private readonly SolutionType type;

        public SolutionOptions(SolutionType type)
        {
            this.type = type;
        }

        public SolutionType Type => type;

        public static SolutionOptions Default => new SolutionOptions(SolutionType.Application);
    }
}
