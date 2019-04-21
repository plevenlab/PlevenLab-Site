using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlevenLab.Data.Entities
{
    public class Event
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int EventId { get; set; }

        public Category Category { get; set; }

        public string Title { get; set; }

        public User CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string LocationFriendlyName { get; set; }

        public float LocationLat { get; set; }

        public float LocationLng { get; set; }
    }
}
