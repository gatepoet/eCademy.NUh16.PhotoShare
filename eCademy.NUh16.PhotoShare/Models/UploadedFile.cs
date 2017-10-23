using System;

namespace eCademy.NUh16.PhotoShare.Models
{
    public class UploadedFile
    {
        public Guid Id { get; set; }
        public byte[] ImageData { get; set; }
        public string Filename { get; set; }
    }
}