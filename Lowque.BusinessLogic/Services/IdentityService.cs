using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Lowque.BusinessLogic.Dto.Identity;
using Lowque.BusinessLogic.Services.Interfaces;
using Lowque.DataAccess;
using Lowque.DataAccess.Entities.Identity;
using Lowque.DataAccess.Internationalization.Interfaces;
using Lowque.DataAccess.Identity;

namespace Lowque.BusinessLogic.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly AppDbContext dbContext;
        private readonly IUserContext userContext;
        private readonly IJwtGenerator jwtGenerator;
        private readonly IPasswordHasher<User> passwordHasher;
        private readonly ILocalizationContext localizationContext;

        public IdentityService(AppDbContext dbContext,
            IUserContext userContext,
            IJwtGenerator jwtGenerator,
            IPasswordHasher<User> passwordHasher,
            ILocalizationContext localizationContext)
        {
            this.dbContext = dbContext;
            this.userContext = userContext;
            this.jwtGenerator = jwtGenerator;
            this.passwordHasher = passwordHasher;
            this.localizationContext = localizationContext;
        }

        public LoginResponseDto Login(LoginRequestDto dto)
        {
            var userToLoginAs = dbContext.Users
                .Include(user => user.Roles)
                .SingleOrDefault(user => user.Email == dto.Email && user.EmailConfirmed);
            if (userToLoginAs == null)
            {
                return new LoginResponseDto
                {
                    Success = false
                };
            }

            var passwordVerificationResult = passwordHasher.VerifyHashedPassword(userToLoginAs, userToLoginAs.PasswordHash, dto.Password);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                return new LoginResponseDto
                {
                    Success = false
                };
            }

            var jwt = jwtGenerator.Generate(userToLoginAs.Email, userToLoginAs.Roles.Select(role => role.Name).ToArray());
            return new LoginResponseDto
            {
                Success = true,
                Jwt = jwt
            };
        }

        public ChangePasswordResponseDto ChangePassword(ChangePasswordRequestDto dto)
        {
            if (dto.NewPassword != dto.NewPasswordConfirmation)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Error = localizationContext.TryTranslate("Profile_Error_PasswordsNotSame")
                };
            }

            var currentUser = dbContext.Users.Single(user => user.Email == userContext.GetUsername());
            var currentPasswordVerificationResult = passwordHasher.VerifyHashedPassword(
                currentUser, currentUser.PasswordHash, dto.CurrentPassword);

            if (currentPasswordVerificationResult != PasswordVerificationResult.Success)
            {
                return new ChangePasswordResponseDto
                {
                    Success = false,
                    Error = localizationContext.TryTranslate("Profile_Error_InvalidPassword")
                };
            }

            currentUser.PasswordHash = passwordHasher.HashPassword(currentUser, dto.NewPassword);
            dbContext.SaveChanges();

            return new ChangePasswordResponseDto
            {
                Success = true
            };
        }
    }
}
