using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestr.Controllers.Data
{
    public class RegistrationRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
