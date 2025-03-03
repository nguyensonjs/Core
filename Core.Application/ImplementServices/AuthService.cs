using AutoMapper;
using Core.Application.InterfaceServices;
using Core.Application.Payloads.RequestModels.UserRequest;
using Core.Application.Payloads.ResponseModels.DataUser;
using Core.Application.Payloads.Responses;
using Core.Domain.Interfaces;
using Core.Domain.Models;
using Core.Domain.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Bcrypt = BCrypt.Net.BCrypt;

namespace Core.Application.ImplementServices
{
    public class AuthService : IAuthService
    {
        private readonly IBaseRepo<User> _baseRepo;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUserRepo _userRepo;

        public AuthService(IBaseRepo<User> baseRepo,
                           IMapper mapper,
                           IConfiguration configuration,
                           IUserRepo userRepo)
        {
            _baseRepo = baseRepo;
            _mapper = mapper;
            _configuration = configuration;
            _userRepo = userRepo;
        }

        public async Task<ResponseObject<UserDTO>> Register(Request_Register request)
        {
            try
            {
                if (!ValidateInput.IsValidEmail(request.Email))
                {
                    return new ResponseObject<UserDTO>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Định dạng email không hợp lệ",
                        Data = null
                    };
                }

                if (!ValidateInput.IsValidPhoneNumber(request.PhoneNumber))
                {
                    return new ResponseObject<UserDTO>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Định dạng số điện thoại không hợp lệ",
                        Data = null
                    };
                }

                if (await _userRepo.GetUserByEmail(request.Email) != null)
                {
                    return new ResponseObject<UserDTO>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Email đã tồn tại. Vui lòng dùng email khác",
                        Data = null
                    };
                }

                if (await _userRepo.GetUserByPhoneNumber(request.PhoneNumber) != null)
                {
                    return new ResponseObject<UserDTO>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Số điện thoại đã tồn tại. Vui lòng dùng SĐT khác",
                        Data = null
                    };
                }

                if (await _userRepo.GetUserByUsername(request.UserName) != null)
                {
                    return new ResponseObject<UserDTO>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "UserName đã tồn tại. Vui lòng dùng UserName khác",
                        Data = null
                    };
                }
                var user = new User
                {
                    AvatarImage = "https://photosbulk.com/wp-content/uploads/2024/08/hijab-girl-pic_108.webp",
                    CreatedTime = DateTime.Now,
                    DateOfBirth = request.DateOfBirth,
                    Email = request.Email,
                    UserName = request.UserName,
                    FullName = request.UserName,
                    PhoneNumber = request.PhoneNumber,
                    Password = Bcrypt.HashPassword(request.Password),
                    Status = Domain.Enums.UserStatus.Inactive,
                };
                user = await _baseRepo.CreateAsync(user);

                await _userRepo.AddRoleToUser(user, new List<string> { "User" });

                var userDTO = _mapper.Map<UserDTO>(user);
                return new ResponseObject<UserDTO>
                {
                    Status =StatusCodes.Status201Created,
                    Message= "Đăng kí thành công",
                    Data = userDTO
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<UserDTO>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi server: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}
