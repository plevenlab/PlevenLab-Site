using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlevenLab.Data.Entities
{
    public class Post
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column(Order = 0)]
        public int PostId { get; set; }

        public string Title { get; set; }

        public string Body { get; set; }

        public Category Category { get; set; }

        public User Creator { get; set; }

        public bool IsVisible { get; set; }

        public DateTime CreationDate { get; set; }
    }
}
