using System.ComponentModel.DataAnnotations;

namespace Integraciones.Models;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public int idEstablecimeinto { get; set; } = 1;
}
