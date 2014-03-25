using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class CreateSiteViewModel
    {
        [Required]
        [DisplayName("Site Name")]
        public string SiteName { get; set; }
        public string Subscription { get; set; }
        [Required]
        [DisplayName("Website Region Name")]
        public string WebSpaceName { get; set; }
        public string WebSpaceGeo { get; set; }
        public string CertPath { get; set; }
    }
}