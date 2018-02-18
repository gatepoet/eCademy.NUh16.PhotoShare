using System;
using System.Collections.Generic;
using System.Linq;

namespace eCademy.NUh16.PhotoShare.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual UploadedFile File { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<UserRating> Ratings { get; set; }

        public double GetScore()
        {
            return Ratings.Any() ? Ratings.Average(rating => rating.Rating) : 0;
        }

        public int GetRating(string userId)
        {
            return Ratings.SingleOrDefault(rating => rating.User.Id == userId)?.Rating ?? 0;
        }

        public void Rate(int value, ApplicationUser user)
        {
            var rating = Ratings.SingleOrDefault(r => r.User == user);
            if (rating == null)
            {
                Ratings.Add(new UserRating {
                    Id = Guid.NewGuid(),
                    Image = this,
                    Rating = value,
                    User = user
                });
            } else
            {
                rating.Rating = value;
            }
        }
    }

    public class UserRating
    {
        public Guid Id { get; set; }
        public virtual Image Image { get; set; }
        public virtual ApplicationUser User { get; set; }
        public int Rating { get; set; }
    }
}