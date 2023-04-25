using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SparklingHome.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the SparklingHomeUser class
    public class SparklingHomeUser : IdentityUser
    {
        [PersonalData]
        public string UserFullName { get; set; }
        [PersonalData]
        public string UserPhoneNumber { get; set; }
        [PersonalData]
        public string UserType { get; set; }
    }
}
