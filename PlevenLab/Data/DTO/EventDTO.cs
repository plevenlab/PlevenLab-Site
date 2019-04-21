using System;

namespace PlevenLab.Data.DTO
{
    public class EventDTO
    {
        public int CategoryId { get; set; }

        public string Title { get; set; }

        public int CreatedByUserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string LocationFriendlyName { get; set; }

        public float LocationLat { get; set; }

        public float LocationLng { get; set; }
    }
}
