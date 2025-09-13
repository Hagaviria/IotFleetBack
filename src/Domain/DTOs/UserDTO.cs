using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs
{
    public class UserDTO
    {
        [Key]
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAtDate { get; set; }
        public DateTime UpdatedAtDate { get; set; }
        public bool USU_ESTADO { get; set; }
    }
}
