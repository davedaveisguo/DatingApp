using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingAPP.API.Helpers;
using DatingAPP.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingAPP.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }



        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u=>u.UserId == userId).FirstOrDefaultAsync(p=>p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
             var photo=await _context.Photos.FirstOrDefaultAsync(p=>p.Id==id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user=await _context.Users.FirstOrDefaultAsync(u=>u.Id==id);

            return user;
        }


        public async Task<PageList<User>> GetUsers(UserParams userParams)
        {
             var users=  _context.Users.OrderByDescending(u=>u.LastActive).AsQueryable();

             users = users.Where(u=>u.Id != userParams.UserId);

             users = users.Where(u=>u.Gender== userParams.Gender);

             if(userParams.Likers)
             {
                var userLikes = await GetUserLikes(userParams.UserId, userParams.Likers);
                users= users.Where(u=>userLikes.Contains(u.Id));
             }

             if(userParams.Likees)
             {
                 var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users= users.Where(u=>userLikees.Contains(u.Id));
             }

             if(userParams.MinAge !=18 || userParams.MaxAge !=99)
             {
                 var minDob = DateTime.Today.AddYears(-userParams.MaxAge-1);
                 var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

                 users = users.Where(u=>u.DateOfBirth >=minDob && u.DateOfBirth<=maxDob);
             }

             if(!string.IsNullOrEmpty(userParams.OrderBy))
             {
                 switch(userParams.OrderBy)
                 {
                     case "created":
                        users=users.OrderByDescending(u=>u.Created);
                        break;
                     default:
                        users=users.OrderByDescending(u=>u.LastActive);
                        break;
                 }
             }

            return await PageList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }


        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            // get the loggedin user with 
            var user = await _context.Users
                       .FirstOrDefaultAsync(u=>u.Id ==id);
          
            if(likers)
            {
                  // return all the likers of current logged in user 
                return user.Likers.Where(u=>u.LikeeId == id).Select(i=>i.LikerId);
            }else
            {
                // return all the likees of current logged in user 
                return user.Likees.Where(u=>u.LikerId==id).Select(i=>i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() >0;
        }



        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u=>u.LikerId ==userId && u.LikeeId==recipientId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m=>m.Id == id);
        }

        public async Task<PageList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                          .AsQueryable();
            
            // filter out message 
            switch(messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u=>u.RecipientId == messageParams.UserId && u.RecipientDeleted==false);
                    break;
                case "Outbox":
                    messages = messages.Where(u=>u.SenderId == messageParams.UserId && u.SenderDeleted ==false);
                    break;
                default:
                     messages = messages.Where(u=>u.RecipientId == messageParams.UserId && u.IsRead ==false 
                     && u.RecipientDeleted==false);
                    break;
            }

            messages = messages.OrderByDescending(d=>d.MessageSent);
            return await PageList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {               // return conversations btw two users
                        var messages = await _context.Messages
                          .Where(m=>m.RecipientId == userId && m.RecipientDeleted ==false 
                          && m.SenderId ==recipientId 
                          || m.RecipientId ==recipientId 
                          && m.SenderId ==userId
                          && m.SenderDeleted==false)
                          .OrderByDescending(m=>m.MessageSent)
                          .ToListAsync();
                        
                        return messages;
        }
    }
}