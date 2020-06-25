namespace DatingAPP.API.Models
{
    public class Like
    {
        // like other user
         public int LikerId { get; set; }

        // being liked by other user
         public int LikeeId { get; set; }

         public User Liker { get; set; }

         public User Likee { get; set; }
    }
}