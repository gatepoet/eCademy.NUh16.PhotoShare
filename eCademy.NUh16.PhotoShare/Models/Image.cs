using System;
using System.Collections.Generic;

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
            ///TODO: Calculate the average of all ratings for this photo.
            var score = new Random().NextDouble() * 5.0;

            return score;
        }

        public int GetRating(string userId)
        {
            ///TODO: Get rating for specific user. Return zero if user has not rated.
            var rating = new Random().Next(1, 5);

            return rating;
        }

        public void Rate(int value, ApplicationUser user)
        {
            ///TODO: Add or update rating for user.
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