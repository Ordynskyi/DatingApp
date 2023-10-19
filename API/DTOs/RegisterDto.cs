using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public struct RegisterDto
{
    [Required]
    public string Username { get; set;}
    [Required]
    [StringLength(maximumLength:64, MinimumLength = 4)]
    public string Password { get; set;}
}
