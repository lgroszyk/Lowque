namespace Lowque.BusinessLogic.Dto
{
    public class DtoValidation
    {
        public const string PascalCaseRegex = "[A-Z][a-z]+(?:[A-Z][a-z]+)*";
        public const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*_=+-]).{8,}$";
    }
}
