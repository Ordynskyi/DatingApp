using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public struct RegisterDto
{
    [Required]
    public string Username { get; set; }
    [Required]
    public string DisplayName { get; set; }
    [Required]
    public DateOnly DateOfBirth { get; set; }
    [Required]
    public string City { get; set; }
    [Required]
    public string Country { get; set; }


    [Required]
    [StringLength(maximumLength:64, MinimumLength = 4)]
    public string Password { get; set;}
}
