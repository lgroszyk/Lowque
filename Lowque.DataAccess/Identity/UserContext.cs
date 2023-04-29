using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Lowque.DataAccess.Identity
{
    public class UserContext : IUserContext
    {
        private readonly AppDbContext dbContext;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UserContext(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetUsername()
        {
            var currentUser = httpContextAccessor.HttpContext.User;
            if (currentUser.Identity.IsAuthenticated)
            {
                return currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
            return null;
        }

        public int GetId()
        {
            var username = GetUsername();
            if (username == null)
            {
                return default;
            }
            return dbContext.Users.Single(user => user.Email == username).UserId;
        }

    }
}
