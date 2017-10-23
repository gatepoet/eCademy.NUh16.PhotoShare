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
    }
}