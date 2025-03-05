using AutoMapper;
using Core.Application.Handle.HandleEmail;
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
        private readonly IEmailService _emailService;
        private readonly IBaseRepo<ConfirmEmail> _baseEmailRepo;

        public AuthService(IBaseRepo<User> baseRepo,
                           IMapper mapper,
                           IConfiguration configuration,
                           IUserRepo userRepo,
                           IEmailService emailService,
                           IBaseRepo<ConfirmEmail> baseEmailRepo)
        {
            _baseRepo = baseRepo;
            _mapper = mapper;
            _configuration = configuration;
            _userRepo = userRepo;
            _emailService = emailService;
            _baseEmailRepo = baseEmailRepo;
        }

        public async Task<string> ConfirmRegisterAccount(string code)
        {
            try
            {
                var cfCode = await _baseEmailRepo.GetFirstOrDefaultAsync(x => x.ConfirmCode.Equals(code));
                if (cfCode == null) {
                    return "Mã xác nhận không hợp lệ";
                }
                var user = await _baseRepo.GetFirstOrDefaultAsync(x => x.Id == cfCode.UserId);
                if(cfCode.ExpiryTime < DateTime.Now)
                {
                    return "Mã xác nhận đã hết hạn";
                };
                user.Status = Domain.Enums.UserStatus.Active;
                cfCode.IsEmailConfirmed = true;
                await _baseRepo.UpdateAsync(user);
                await _baseEmailRepo.UpdateAsync(cfCode);
                return "Xác nhận đăng ký tài khoản thành công";
            }
            catch (Exception ex) {
                return ex.Message;
            }
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

                string activationCode = GenerateCodeActive();
                ConfirmEmail cfEmail = new ConfirmEmail
                {
                    ConfirmCode = activationCode,
                    ExpiryTime = DateTime.Now.AddMinutes(1),
                    IsEmailConfirmed = false,
                    UserId = user.Id,
                };
                cfEmail = await _baseEmailRepo.CreateAsync(cfEmail);

                string emailResult = _emailService.SendVerificationEmail(request.Email, activationCode);
                if (!emailResult.Contains("successfully"))
                {
                    return new ResponseObject<UserDTO>
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Message = $"Đăng ký thành công nhưng gửi email thất bại: {emailResult}",
                        Data = null
                    };
                }

                var userDTO = _mapper.Map<UserDTO>(user);
                return new ResponseObject<UserDTO>
                {
                    Status =StatusCodes.Status201Created,
                    Message= "Đăng ký thành công. Vui lòng kiểm tra email để kích hoạt tài khoản.",
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

        private string GenerateCodeActive()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
