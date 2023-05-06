using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using SparklingHome.Areas.Identity.Data;

namespace SparklingHome.Models
{
    public enum TIMESLOT { 
        [Description("Morning")]
        MORNING,
        [Description("Afternoon")]
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

        public TIMESLOT Timeslot { get; set; }

        public SERVICE_TYPE ServiceType { get; set; }

        public Boolean ReservationStatus { get; set; }

        [ForeignKey("AspNetUsers")]
        public string CustomerID { get; set; }

        public SparklingHomeUser Customer { get; set; }

    }
}
