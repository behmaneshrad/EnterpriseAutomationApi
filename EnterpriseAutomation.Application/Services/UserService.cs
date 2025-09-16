using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Models.Users;
using EnterpriseAutomation.Application.Services.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using System.Security.Claims;


namespace EnterpriseAutomation.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> _userRepository;
        public UserService(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }


        //public async Task<UserDto?> GetUserByUsernameAsync(string username)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        //    if (user == null)
        //        return null;

        //    return new UserDto
        //    {
        //        Username = user.Username,
        //        Role = user.Role
        //        // Add Email if needed
        //    };
        //}

        public async Task CreateUserAsync(User user)
        {
            //_context.Users.Add(user);
            //await _context.SaveChangesAsync();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            await _userRepository.InsertAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        #region Get User Login
        public async Task<UserDto?> GetCurrentUserAsync(ClaimsPrincipal userClaims)
        {
            var username = userClaims.Identity?.Name;

            if (string.IsNullOrEmpty(username))
                return null;

            var user = await _userRepository.GetFirstOrDefaultAsync(x => x.Username == username);

            if (user == null)
                return null;

            return new UserDto
            {
                Username = user.Username,
                Role = user.Role
            };
        }
        #endregion

        #region Test Get user

        public async Task<UserDto> GetUserByUserNameAsync(string userName)
        {
            
            if (string.IsNullOrEmpty(userName))
                return null;

            var user = await _userRepository.GetFirstOrDefaultAsync(x => x.Username == userName);

            if (user == null)
                return null;

            return new UserDto
            {
                Username = user.Username,
                Role = user.Role
            };
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {           

            var user = await _userRepository.GetFirstOrDefaultAsync(x => x.UserId == userId);

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username                
            };
        }

        public async Task<IEnumerable<UserDto>> GetAllUserAsync()
        {
            var users = await _userRepository.GetAllAsync();

            var res = users.Select(a => new UserDto
            {
                Email = "",
                Username = a.Username,
                Role = a.Role
            });

            return res;
        }
        #endregion

        //public async Task<User?> ValidateUserAsync(string username, string password)
        //{
        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Username == username);

        //    if (user == null)
        //        return null;

        //    var passwordHasher = new PasswordHasher<User>();
        //    var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

        //    if (result == PasswordVerificationResult.Failed)
        //        return null;

        //    return user;
        //}
    }
}
