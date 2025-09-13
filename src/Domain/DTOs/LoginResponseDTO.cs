using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs
{
    public class LoginResponseDTO
    {
        [Required]
        public required string Token { get; set; } = string.Empty;
        [Required]
        public required string Identificacion { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Nombre { get; set; }
        public string NombreIPS { get; set; }
        public string CodigoIPS { get; set; }
        public string CodigoHabilitacion { get; set; }
        public int IdPerfil { get; set; }
        public string NombrePerfil { get; set; }
    }
}
