using System;

namespace eCademy.NUh16.PhotoShare.Droid
{
    public class Photo
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }
        public DateTime Timestamp { get; set; }
        public string PhotoUrl { get; set; }
        public int Rating { get; set; }
        public double Score { get; set; }
    }
}