using Core.Domain.Models;

namespace Core.Domain.Interfaces
{
    public interface IUserRepo
    {
        Task<User>GetUserByEmail(string email);
        Task<User> GetUserByUsername(string username);
        Task<User> GetUserByPhoneNumber(string phoneNumber);
        Task AddRoleToUser (User user, List<string> listRoles);
        Task<IEnumerable<string>> GetUserRoles (User user);
    }
}
