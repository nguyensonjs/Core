using Core.Application.Payloads.ResponseModels.DataUser;
using Core.Domain.Models;

namespace Core.Application.Payloads.Mappers
{
    public class UserConverter
    {
        public UserDTO ConvertToDto(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                AvatarImage = user.AvatarImage,
                Email = user.Email,
                CreatedTime = user.CreatedTime,
                DateOfBirth = user.DateOfBirth,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                UpdatedTime = user.UpdatedTime,
                Status = user.Status.ToString()
            };
        }
    }
}
