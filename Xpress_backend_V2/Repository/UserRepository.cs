using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using Xpress_backend_V2.Data;
using Xpress_backend_V2.Interface;
using Xpress_backend_V2.Models;
using Xpress_backend_V2.Models.DTO;

namespace Xpress_backend_V2.Repository
{
    public class UserRepository : IUserServices , IUserRepository
    {
        private readonly ApiDbContext _context;

        public UserRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Include(u => u.TravelRequests)
                .Include(u => u.CreatedTicketOptions)
                .Where(u => u.IsActive)
                .ToListAsync();
        }

        public async Task<User> GetByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.TravelRequests)
                .Include(u => u.CreatedTicketOptions)
                .FirstOrDefaultAsync(u => u.UserId == userId && u.IsActive);
        }

        public async Task AddAsync(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.IsActive = true;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User> GetByEmployeeNameAsync(string employeeName)
        {
            return await _context.Users
                .Include(u => u.TravelRequests)
                .Include(u => u.CreatedTicketOptions)
                .FirstOrDefaultAsync(u => u.EmployeeName == employeeName && u.IsActive);
        }


        public async Task<User> LoginUser(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeEmail == email);

            if (user != null)
            {
                if (user.Password == HashPassword(password))
                    return user;
            }

            return null;
        }

        public async Task<User> RegisterUser(UserRegisterDTO user)
        {
            try
            {
                Console.WriteLine($"Attempting to register user: {user.EmployeeEmail}");

                // Check if user already exists
                var userExist = await _context.Users
                    .FirstOrDefaultAsync(u => u.EmployeeEmail == user.EmployeeEmail);

                if (userExist != null)
                {
                    Console.WriteLine($"User already exists with email: {user.EmployeeEmail}");
                    return null;
                }

                // Create new user WITHOUT setting UserId
                var newUser = new User
                {
                    // DO NOT SET UserId - let it auto-generate
                    EmployeeName = user.EmployeeName,
                    EmployeeEmail = user.EmployeeEmail,
                    Password = HashPassword(user.Password),
                    PhoneNumber = user.PhoneNumber ?? "",
                    UserRole = user.UserRole,
                    Department = user.Department,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                Console.WriteLine($"Adding user to context (UserId should be 0): {newUser.UserId}");

                _context.Users.Add(newUser);

                Console.WriteLine("Calling SaveChangesAsync...");
                await _context.SaveChangesAsync();

                Console.WriteLine($"User saved successfully with UserId: {newUser.UserId}");
                return newUser;
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pgEx)
            {
                Console.WriteLine($"PostgreSQL Error - SqlState: {pgEx.SqlState}, Constraint: {pgEx.ConstraintName}");
                Console.WriteLine($"Error Message: {pgEx.MessageText}");

                if (pgEx.SqlState == "23505" && pgEx.ConstraintName == "PK_Users")
                {
                    Console.WriteLine("Primary key violation detected. This indicates a sequence sync issue.");
                    // Log current state for debugging
                    var maxId = await _context.Users.MaxAsync(u => (int?)u.UserId) ?? 0;
                    Console.WriteLine($"Current max UserId in database: {maxId}");
                }

                throw; // Re-throw to see the full error
            }
            catch (Exception e)
            {
                Console.WriteLine($"General Error: {e.Message}");
                Console.WriteLine($"Stack Trace: {e.StackTrace}");
                throw;
            }
        }
        public string HashPassword(string password)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hashedString = sha512.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedString);
            }
        }








    }
}
