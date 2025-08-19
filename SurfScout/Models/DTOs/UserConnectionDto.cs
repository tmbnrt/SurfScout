using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SurfScout.Services;

namespace SurfScout.Models.DTOs
{
    // Addressee and requester can change (request- or accept- DTO)
    class UserConnectionDto
    {
        public int RequesterId { get; set; }
        public int AddresseeId { get; set; }
        public string RequesterUsername { get; set; }
        public string AddresseeUsername { get; set; }
    }
}
