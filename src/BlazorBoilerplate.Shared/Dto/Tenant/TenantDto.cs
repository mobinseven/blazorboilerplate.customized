using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorBoilerplate.Shared.Dto.Tenant
{
    public class TenantDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public Guid ManagerUserId { get; set; }
    }
}