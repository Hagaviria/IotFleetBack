using System;
using System.ComponentModel.DataAnnotations;

namespace Application.Features.User.Command
{
    /// <summary>
    /// Command to create a new user.
    /// </summary>
    public sealed record UserCreateCommand
    {
        [Required]
        public string Identificacion { get; set; }

        [Required]
        public string Nombre_completo { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        public string Correo { get; set; }

        [Required]
        public string Contraseña { get; set; }

        public int Id_perfil { get; set; }

        public bool Estado { get; set; }

        public DateTime? Creado_en { get; set; }

     

        public string Direccion { get; set; }

        public string TelefonoFijo { get; set; }

        public string TelefonoCelular { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string TipoIdentificacion { get; set; }
    }

    /// <summary>
    /// Command to update a user.
    /// </summary>
    public sealed record UserUpdateCommand
    {
        [Required]
        public string Identificacion { get; set; }

        [Required]
        public string Nombre_completo { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        public string Correo { get; set; }

        [Required]
        public string Contraseña { get; set; }

        public int Id_perfil { get; set; }

        public bool Estado { get; set; }

        public DateTime? Creado_en { get; set; }

  
        public string Direccion { get; set; }

        public string TelefonoFijo { get; set; }

        public string TelefonoCelular { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        public string TipoIdentificacion { get; set; }
    }

    public sealed record ChangePasswordCommand
    {
        [Required]
        public string OldPassword { get; init; }
        [Required]
        public string NewPassword { get; init; }
    }
}
