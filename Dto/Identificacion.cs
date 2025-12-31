using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integraciones.Application.DTOs
{

  
    public class IdentificacionDto
    {


        public int idRegistro { get; set; }
        public required string txtIdentificacion { get; set; }
        public required string txtTipoIdentificacion { get; set; }
        public string? txtNombresApellidos { get; set; }
        public DateTime? txtFechaIngreso { get; set; }
        public  string? txtUsuarioIngreso { get; set; }
        public required bool txtestado { get; set; }
        public required bool bdVerificado { get; set; }


    }

    public class IdentificacionEstadoDto
    {
        public bool txtestado { get; set; }              // Nuevo estado
   
    }


    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        public int TotalPages =>
            (int)Math.Ceiling((double)TotalItems / PageSize);
    }

}
