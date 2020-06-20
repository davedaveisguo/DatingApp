using System.ComponentModel.DataAnnotations;

namespace DatingAPP.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength=4, ErrorMessage="You must specify pwd btw 4 and 8")]
        public string Password { get; set; }
    }
}