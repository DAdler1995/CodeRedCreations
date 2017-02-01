using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace CodeRedCreations.Models.Account
{
    public class UserReferral
    {
        [DataMember, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public bool Enabled { get; set; }
        public string ReferralCode { get; set; }
        public decimal Earnings { get; set; }
        [EmailAddress(ErrorMessage = "Not a valid email address.")]
        public string PayPalAccount { get; set; }
        public bool RequestedPayout { get; set; }

        [DefaultValue(33)]
        public int PayoutPercent { get; set; }

        [DefaultValue(5)]
        public int StoreCreditPercent { get; set; }
    }
}
