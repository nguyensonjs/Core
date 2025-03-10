using AutoMapper;
using Core.Application.Payloads.ResponseModels.DataUser;
using Core.Domain.Models;

namespace Core.Application.Payloads.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile() {
            CreateMap<User,UserDTO>();
            CreateMap<User, UserLoginDTO>();
        }
    }
}
