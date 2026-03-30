using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Shared.DTOs.Users
{
    public class UserListRequestDto
    {
        public string? Search { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "CreatedAt";
        public string SortDirection { get; set; } = "DESC";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
