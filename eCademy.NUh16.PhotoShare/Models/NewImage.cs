using System.ComponentModel.DataAnnotations;
using System.Web;

namespace eCademy.NUh16.PhotoShare.Models
{
    public class NewImage
    {
        public string Title { get; set; }
        [Required]
        public HttpPostedFileBase File { get; set; }
    }
}
