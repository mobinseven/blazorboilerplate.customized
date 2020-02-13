using System;
using System.ComponentModel.DataAnnotations;

namespace BlazorBoilerplate.Server.Models
{
    public class UserProfile
    {
        [Key]
        public long Id { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime LastUpdatedDate { get; set; } = DateTime.MinValue;
    }
}