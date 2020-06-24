using System;
using System.ComponentModel.DataAnnotations;

namespace DatingAPP.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify pwd btw 4 and 8")]
        public string Password { get; set; }

        [Required]
        public string Gender { get; set; }
        [Required]
        public string KnownAs { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        [Required]
        public string City { get; set; }

        public DateTime Created { get; set; }

        public DateTime lastActive { get; set; }

        public UserForRegisterDto()
        {
            Created = DateTime.Now;
            lastActive = DateTime.Now;
        }
    }
}