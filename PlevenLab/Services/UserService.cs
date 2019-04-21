using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlevenLab.Data;
using PlevenLab.Data.DTO;
using PlevenLab.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlevenLab.Services
{
    public interface IUserService
    {
        void CreateAdminUser();

        Task<User> LoginAsync(string username, string password);

        User GetById(int id);

        Task<User> GetByIdAsync(int id);

        Task<User> CreateAsync(UserDTO userDTO);

        User Create(UserDTO userDTO);

        Task<User> UpdateAsync(int id, UserDTO userDTO);
    }

    public class UserService : IUserService
    {
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context, ILogger<Startup> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void CreateAdminUser()
        {
            var users = _context.Users.Count();
            if (users == 0)
            {
                var password = GenerateRandomPassword(new PasswordOptions
                {
                    RequiredLength = 16,
                    RequiredUniqueChars = 8,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = true,
                    RequireUppercase = true
                });
                var user = Create(new UserDTO
                {
                    Name = "admin",
                    Email = "admin@plevenlab.org",
                    Password = password,
                });
                _logger.LogWarning("Automatically created user: {0} password: {1}", user.Name, password);
            }
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Name == username);
            if (user == null)
            {
                return null;
            }

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            // authentication successful
            return user;
        }

        public User GetById(int id)
        {
            return _context.Users.Find(id);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public User Create(UserDTO userDTO)
        {
            // validation
            if (string.IsNullOrWhiteSpace(userDTO.Password))
            {
                throw new AppException("Password is required");
            }

            if (_context.Users.Any(x => x.Name == userDTO.Name))
            {
                throw new AppException("Username \"" + userDTO.Name + "\" is already taken");
            }

            CreatePasswordHash(userDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
            };

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public async Task<User> CreateAsync(UserDTO userDTO)
        {
            // validation
            if (string.IsNullOrWhiteSpace(userDTO.Password))
            {
                throw new AppException("Password is required");
            }

            if (await _context.Users.AnyAsync(x => x.Name == userDTO.Name))
            {
                throw new AppException("Username \"" + userDTO.Name + "\" is already taken");
            }

            CreatePasswordHash(userDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Name = userDTO.Name,
                Email = userDTO.Email,
            };

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateAsync(int id, UserDTO userDTO)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }

            if (user.Name != userDTO.Name && await _context.Users.AnyAsync(x => x.Name == userDTO.Name))
            {
                throw new AppException("Username \"" + userDTO.Name + "\" is already taken");
            }

            user.Name = userDTO.Name;
            user.Email = userDTO.Email;
            if (!string.IsNullOrWhiteSpace(userDTO.Password))
            {
                CreatePasswordHash(userDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }


        /// <summary>
        /// Generates a Random Password
        /// respecting the given strength requirements.
        /// </summary>
        /// <param name="opts">A valid PasswordOptions object
        /// containing the password strength requirements.</param>
        /// <returns>A random password</returns>
        private static string GenerateRandomPassword(PasswordOptions opts = null)
        {
            if (opts == null)
            {
                opts = new PasswordOptions()
                {
                    RequiredLength = 8,
                    RequiredUniqueChars = 4,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireNonAlphanumeric = true,
                    RequireUppercase = true
                };
            }

            string[] randomChars = new[] {
            "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
            "abcdefghijkmnopqrstuvwxyz",    // lowercase
            "0123456789",                   // digits
            "!@$?_-"                        // non-alphanumeric
        };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);
            }

            if (opts.RequireLowercase)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);
            }

            if (opts.RequireDigit)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);
            }

            if (opts.RequireNonAlphanumeric)
            {
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);
            }

            for (int i = chars.Count; i < opts.RequiredLength
                || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }


        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            else if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            }
            else if (storedHash.Length != 64)
            {
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            }
            else if (storedSalt.Length != 128)
            {
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");
            }

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
