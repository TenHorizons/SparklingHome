using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SparklingHome.Models
{
    public class Maid
    {
        [Key]
        public int MaidId { get; set; }

        [Required(ErrorMessage = "You have not provided a name for the new employee")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "You have not provided the registration date for this employee")]
        public DateTime RegistrationDate { get; set; }

        [Required(ErrorMessage = "You have not specified the employee's gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "The employees contact information is required for administrative purposes")]
        public string ContactNo { get; set; }

        [Required(ErrorMessage = "The employee's monthly salary is required for auditing purposes")]
        [Range(2000, 5000, ErrorMessage = "The employee's wage cannot be less than RM2000 or more than RM5000")]
        public decimal Salary { get; set; }

        public Boolean IsAvailable { get; set; }

        public int WorkingExperienceInYears { get; set; }
    }
}
