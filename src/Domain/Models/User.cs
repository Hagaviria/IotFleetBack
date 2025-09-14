using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Domain.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public string Identificacion { get; set; }
        public string Nombre_completo { get; set; }
        public string Correo { get; set; }
        public string PasswordHash { get; set; }
        public int Id_perfil { get; set; }
        public string Nombre_perfil { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime Creado_en { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Direccion { get; set; }
        public string TelefonoFijo { get; set; }
        public string TelefonoCelular { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string TipoIdentificacion { get; set; }
    }
}
