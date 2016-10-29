using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CodeRedCreations.Models
{
    public class ProductRequestModel
    {
        [Key]
        public int RequestId { get; set; }
        [Required]
        public virtual ProductModel Part { get; set; }

        [EmailAddress, Display(Name = "Email Address")]
        public string FromEmail { get; set; }
    }
}
