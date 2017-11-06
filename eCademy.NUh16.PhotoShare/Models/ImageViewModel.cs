using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace eCademy.NUh16.PhotoShare.Models
{
    public class ImageViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [DataType(DataType.ImageUrl)]
        public string Base64Image { get; set; }
        public DateTime Timestamp { get; set; }
        public int Rating { get; set; }
        public double Score { get; set; }
        public string Username { get; set; }
    }

    public class RateResult
    {
        public RateResult(double newScore)
        {
            NewScore = newScore;
        }
        public double NewScore { get; set; }
    }
}