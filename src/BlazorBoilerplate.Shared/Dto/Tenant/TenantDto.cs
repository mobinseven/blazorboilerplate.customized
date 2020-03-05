using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlazorBoilerplate.Shared.Dto
{
    public class TenantDto
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }
    }
}