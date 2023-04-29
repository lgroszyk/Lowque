namespace Lowque.DataAccess.Identity
{
    public interface IJwtGenerator
    {
        string Generate(string username, string[] roles);
    }
}
