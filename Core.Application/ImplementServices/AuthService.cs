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
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        private readonly IBaseRepo<RefreshToken> _baseRefeshToken;

        public AuthService(IBaseRepo<User> baseRepo,
                          IMapper mapper,
                          IConfiguration configuration,
                          IUserRepo userRepo,
                          IEmailService emailService,
                          IBaseRepo<ConfirmEmail> baseEmailRepo,
                          IBaseRepo<RefreshToken> baseRefeshToken)
        {
            _baseRepo = baseRepo;
            _mapper = mapper;
            _configuration = configuration;
            _userRepo = userRepo;
            _emailService = emailService;
            _baseEmailRepo = baseEmailRepo;
            _baseRefeshToken = baseRefeshToken;
        }

        public async Task<string> ConfirmRegisterAccount(string code)
        {
            try
            {
                var cfCode = await _baseEmailRepo.GetFirstOrDefaultAsync(x => x.ConfirmCode.Equals(code));
                if (cfCode == null)
                    return "Mã xác nhận không hợp lệ";

                if (cfCode.ExpiryTime < DateTime.UtcNow)
                    return "Mã xác nhận đã hết hạn";

                var user = await _baseRepo.GetFirstOrDefaultAsync(x => x.Id == cfCode.UserId);
                if (user == null)
                    return "Người dùng không tồn tại";

                user.Status = Domain.Enums.UserStatus.Active;
                cfCode.IsEmailConfirmed = true;

                await _baseRepo.UpdateAsync(user);
                await _baseEmailRepo.UpdateAsync(cfCode);

                return "Xác nhận đăng ký tài khoản thành công";
            }
            catch (Exception ex)
            {
                return $"Lỗi: {ex.Message}";
            }
        }

        public async Task<ResponseObject<string>> ResendConfirmationCode(string email)
        {
            try
            {
                var user = await _userRepo.GetUserByEmail(email);
                if (user == null)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Người dùng không tồn tại.",
                        Data = null
                    };
                }

                if (user.Status == Domain.Enums.UserStatus.Active)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Tài khoản đã được kích hoạt.",
                        Data = null
                    };
                }

                var existingCode = await _baseEmailRepo.GetFirstOrDefaultAsync(x => x.UserId == user.Id);
                if (existingCode != null)
                {
                    await _baseEmailRepo.DeleteAsync(existingCode.Id);
                }

                string newCode = GenerateCodeActive();
                var confirmEmail = new ConfirmEmail
                {
                    UserId = user.Id,
                    ConfirmCode = newCode,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(10),
                    IsEmailConfirmed = false
                };

                await _baseEmailRepo.CreateAsync(confirmEmail);

                string emailResult = _emailService.SendVerificationEmail(user.Email, newCode);
                if (!emailResult.ToLower().Contains("successfully")) 
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Message = $"Gửi email thất bại: {emailResult}",
                        Data = null
                    };
                }



                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Mã xác nhận đã được gửi lại. Vui lòng kiểm tra email của bạn.",
                    Data = "Mã xác nhận mới đã được gửi."
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi server: {ex.Message}",
                    Data = null
                };
            }
        }
        public async Task<ResponseObject<string>> ChangePassword(Guid userId, Request_ChangePassword request)
        {
            try
            {
                var user = await _baseRepo.GetFirstOrDefaultAsync(x => x.Id == userId);
                if (user == null)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status404NotFound,
                        Message = "Người dùng không tồn tại.",
                        Data = null
                    };
                }

                if (!Bcrypt.Verify(request.OldPassword, user.Password))
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu cũ không chính xác.",
                        Data = null
                    };
                }

                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return new ResponseObject<string>
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = "Mật khẩu mới và xác nhận mật khẩu không khớp.",
                        Data = null
                    };
                }

                user.Password = Bcrypt.HashPassword(request.NewPassword);
                await _baseRepo.UpdateAsync(user);

                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Đổi mật khẩu thành công.",
                    Data = "Mật khẩu đã được cập nhật."
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<string>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi server: {ex.Message}",
                    Data = null
                };
            }
        }


        public async Task<ResponseObject<UserLoginDTO>> GetJwtTokenAsync(User user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                // Lấy roles của user và thêm vào claims
                var roles = await _userRepo.GetUserRoles(user);
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                string token = GenerateToken(claims);

                string refreshToken = GenerateRefreshToken();

                await StoreRefreshToken(user.Id, refreshToken);
                var userLoginDto = _mapper.Map<UserLoginDTO>(user);
                userLoginDto.AccessToken = token;
                userLoginDto.RefreshToken = refreshToken;

                return new ResponseObject<UserLoginDTO>
                {
                    Status = StatusCodes.Status200OK,
                    Message = "Tạo token thành công",
                    Data = userLoginDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseObject<UserLoginDTO>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi server: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseObject<UserLoginDTO>> Login(Request_Login request)
        {
            try
            {
                var user = await _userRepo.GetUserByUsername(request.UserName);
                if (user == null || !Bcrypt.Verify(request.Password, user.Password))
                {
                    return new ResponseObject<UserLoginDTO>
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Message = "Tên đăng nhập hoặc mật khẩu không đúng",
                        Data = null
                    };
                }

                if (user.Status != Domain.Enums.UserStatus.Active)
                {
                    return new ResponseObject<UserLoginDTO>
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Message = "Tài khoản chưa được kích hoạt hoặc bị khóa",
                        Data = null
                    };
                }

                return await GetJwtTokenAsync(user);
            }
            catch (Exception ex)
            {
                return new ResponseObject<UserLoginDTO>
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Message = $"Lỗi server: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<ResponseObject<UserDTO>> Register(Request_Register request)
        {
            try
            {
                if (!ValidateInput.IsValidEmail(request.Email))
                    return CreateBadRequestResponse<UserDTO>("Định dạng email không hợp lệ");

                if (!ValidateInput.IsValidPhoneNumber(request.PhoneNumber))
                    return CreateBadRequestResponse<UserDTO>("Định dạng số điện thoại không hợp lệ");

                if (await _userRepo.GetUserByEmail(request.Email) != null)
                    return CreateBadRequestResponse<UserDTO>("Email đã tồn tại. Vui lòng dùng email khác");

                if (await _userRepo.GetUserByPhoneNumber(request.PhoneNumber) != null)
                    return CreateBadRequestResponse<UserDTO>("Số điện thoại đã tồn tại. Vui lòng dùng SĐT khác");

                if (await _userRepo.GetUserByUsername(request.UserName) != null)
                    return CreateBadRequestResponse<UserDTO>("UserName đã tồn tại. Vui lòng dùng UserName khác");

                var user = new User
                {
                    AvatarImage = "https://photosbulk.com/wp-content/uploads/2024/08/hijab-girl-pic_108.webp",
                    CreatedTime = DateTime.UtcNow,
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
                var cfEmail = new ConfirmEmail
                {
                    ConfirmCode = activationCode,
                    ExpiryTime = DateTime.UtcNow.AddMinutes(1),
                    IsEmailConfirmed = false,
                    UserId = user.Id,
                };
                await _baseEmailRepo.CreateAsync(cfEmail);

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
                    Status = StatusCodes.Status201Created,
                    Message = "Đăng ký thành công. Vui lòng kiểm tra email để kích hoạt tài khoản.",
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

        #region Private Methods
        private string GenerateCodeActive()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateRefreshToken()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 32) // Độ dài 32 ký tự
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task StoreRefreshToken(Guid userId, string refreshToken)
        {
            var refreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            await _baseRefeshToken.CreateAsync(refreshTokenEntity); 
        }

        private JwtSecurityToken GetToken(List<Claim> claims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfiguration:Secret"]));
            return new JwtSecurityToken(
                issuer: _configuration["JwtConfiguration:ValidIssuer"],
                audience: _configuration["JwtConfiguration:ValidAudience"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: claims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
        }

        private string GenerateToken(List<Claim> claims)
        {
            var token = GetToken(claims);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ResponseObject<T> CreateBadRequestResponse<T>(string message)
        {
            return new ResponseObject<T>
            {
                Status = StatusCodes.Status400BadRequest,
                Message = message,
                Data = default(T)
            };
        }
        #endregion
    }
}