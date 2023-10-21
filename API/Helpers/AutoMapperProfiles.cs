using API.DTOs;
using API.Entities;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDto>()
            .ForMember(
                dest => dest.PhotoUrl,
                source => source.MapFrom(src => src.Photos.First(p => p.IsMain).Url))
                // source => source.MapFrom<MainPhotoUrlMapper>())  // this doesn't work with '.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)' in the UserRepository.cs :(
            .ForMember(
                dest => dest.Age,
                source => source.MapFrom(src => src.DateOfBirth.CalculateAge()));

        CreateMap<Photo, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
    }
}
