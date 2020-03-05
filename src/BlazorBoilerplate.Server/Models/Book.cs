using BlazorBoilerplate.Server.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Models
{
    public class Book : ITenant
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [NotMapped]
        public string BookStoreTitle { get; set; }

    }
}