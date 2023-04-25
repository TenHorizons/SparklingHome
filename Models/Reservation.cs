using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SparklingHome.Models
{
    public enum TIMESLOT { 
        MORNING,
        AFTERNOON
    }

    public enum SERVICE_TYPE { 
        SERVICE1,
        SERVICE2
    }

    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [ForeignKey("Maid")]
        public int MaidId { get; set; }

        public Maid Maid { get; set; }

        public string Address { get; set; }

        public int Postcode { get; set; }

        public string Timeslot { get; set; }

        public string ServiceType { get; set; }

        public Boolean ReservationStatus { get; set; }

    }
}
