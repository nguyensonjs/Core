using Core.Domain.Interfaces;
using Core.Domain.Models;
using Core.Infra.DataContexts;
using Microsoft.EntityFrameworkCore;

namespace Core.Infra.Repo
{
    public class UserRepo : IUserRepo
    {
        private readonly AppDbContext _context;

        public UserRepo(AppDbContext context)
        {
            _context = context;
        }

        #region Xử lý chuỗi

        private async Task<bool> IsStringInList(string inputStr, List<string> strings)
        {
            if (inputStr == null) throw new ArgumentNullException(nameof(inputStr));
            if (strings == null) throw new ArgumentNullException(nameof(strings));

            return strings.Any(str => string.Equals(str, inputStr, StringComparison.OrdinalIgnoreCase));
        }
        #endregion

        public async Task AddRoleToUser(User user, List<string> listRoles)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (listRoles == null) throw new ArgumentNullException(nameof(listRoles));

            foreach (var role in listRoles.Distinct())
            {
                var roleOfUser = (await GetRolesForUser(user)).ToList();
                if (await IsStringInList(role, roleOfUser))
                {
                    throw new ArgumentException("Người dùng đã có quyền này rồi");
                }

                var roleItem = await _context.Roles.SingleOrDefaultAsync(x => x.RoleCode.Equals(role));
                if (roleItem == null)
                {
                    throw new ArgumentException($"Không tồn tại quyền {role}");
                }

                _context.Permissions.Add(new Permission
                {
                    RoleId = roleItem.Id,
                    UserId = user.Id
                });
            }

            await _context.SaveChangesAsync(); // ✅ Sử dụng `await` để tránh lỗi
        }

        public async Task<IEnumerable<string>> GetRolesForUser(User user)
        {
            var roles = new List<string>();

            var listRoles = _context.Permissions
                .Where(x => x.UserId == user.Id)
                .Select(x => x.RoleId)
                .Distinct();

            foreach (var roleId in listRoles)
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role != null)
                {
                    roles.Add(role.RoleCode);
                }
            }

            return roles;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u => EF.Functions.Like(u.Email, email));
        }

        public async Task<User> GetUserByPhoneNumber(string phoneNumber)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u => EF.Functions.Like(u.PhoneNumber, phoneNumber));
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u => EF.Functions.Like(u.UserName, username));
        }

    }
}
