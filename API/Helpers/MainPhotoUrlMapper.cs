using API.DTOs;
using API.Entities;
using AutoMapper;

namespace API.Helpers;

public class MainPhotoUrlMapper : IValueResolver<AppUser, MemberDto, string>
{
    public string Resolve(AppUser source, MemberDto destination, string destMember, ResolutionContext context)
    {
        return source.Photos.FirstOrDefault(x => x.IsMain)?.Url ?? string.Empty;
    }
}
