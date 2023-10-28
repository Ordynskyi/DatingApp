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
                source => source.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url))
            // source => source.MapFrom<MainPhotoUrlMapper>())  // this doesn't work with '.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)' in the UserRepository.cs :(
            .ForMember(
                dest => dest.Age,
                source => source.MapFrom(src => src.DateOfBirth.CalculateAge()));

        CreateMap<Photo, PhotoDto>();
        CreateMap<ModerationPhoto, PhotoDto>();
        CreateMap<MemberUpdateDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();

        CreateMap<Message, MessageDto>()
            .ForMember(
                d => d.SenderPhotoUrl, o => o.MapFrom(s => 
                    s.Sender.Photos.FirstOrDefault(x => x.IsMain).Url))
            .ForMember(
                d => d.RecipientPhotoUrl, o => o.MapFrom(s => 
                    s.Recipient.Photos.FirstOrDefault(x => x.IsMain).Url));

        CreateMap<DateTime, DateTime>()
            .ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));

        CreateMap<DateTime?, DateTime?>()
            .ConvertUsing(d => d.HasValue 
                ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : null);
    }
}
