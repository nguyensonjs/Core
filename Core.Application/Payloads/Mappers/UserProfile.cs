using AutoMapper;
using Core.Application.Payloads.ResponseModels.DataUser;
using Core.Domain.Models;

namespace Core.Application.Payloads.Mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
