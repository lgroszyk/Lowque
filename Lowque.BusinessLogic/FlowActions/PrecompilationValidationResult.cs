using System.Collections.Generic;

namespace Lowque.BusinessLogic.FlowActions
{
    public class PrecompilationValidationResult
    {
        public bool IsValid { get; set; }
        public IEnumerable<string> Errors { get; set; }

        public static PrecompilationValidationResult Invalid(string error)
        {
            return new PrecompilationValidationResult
            {
                IsValid = false,
                Errors = new[] { error }
            };
        }

        public static PrecompilationValidationResult Valid()
        {
            return new PrecompilationValidationResult
            {
                IsValid = true
            };
        }
    }
}
