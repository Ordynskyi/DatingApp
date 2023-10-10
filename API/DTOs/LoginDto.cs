using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public struct LoginDto
{
    [Required]
    public string Username { get; set;}
    [Required]
    public string Password { get; set;}
}
