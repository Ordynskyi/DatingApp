using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public struct RegisterDto
{
    [Required]
    public string Username { get; set;}
    [Required]
    public string Password { get; set;}
}
