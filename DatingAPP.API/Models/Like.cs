namespace DatingAPP.API.Models
{
    public class Like
    {
        // like other user
         public int LikerId { get; set; }

        // being liked by other user
         public int LikeeId { get; set; }

         public virtual User Liker { get; set; }

         public virtual User Likee { get; set; }
    }
}