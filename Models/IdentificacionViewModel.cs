using System.ComponentModel.DataAnnotations;

namespace Integraciones.Models;

public class IdentificacionViewModel
{
    public int idRegistro { get; set; }

    [Required(ErrorMessage = "Ingrese identificaci�n")]
    [Display(Name = "N�mero de C�dula/RUC")]
    public string txtIdentificacion { get; set; } = string.Empty;


    [Required(ErrorMessage = "Ingrese el tipo de identificaci�n")]
    [Display(Name = "Tipo de identificaci�n")]
    public string txtTipoIdentificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese Nombres y Apellidos")]
    [Display(Name = "Nombres y Apellidos")]
    public string txtNombresApellidos { get; set; } = string.Empty;

    public bool txtestado { get; set; }


    public DateTime txtFechaIngreso { get; set; }
    public string txtUsuarioIngreso { get; set; } = string.Empty;

}


public class IdentificacionEstadoViewModel
{
    public int idRegistro { get; set; }
    public bool txtestado { get; set; }
}



