using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Niteco.Web.Models.ViewModels
{
    public class ContactFormModel
    {
        [Required]
        public string FullName { get; set; }
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }
        public string Message { get; set; }
        public string Country { get; set; }
    }
}