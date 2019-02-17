using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SynkerIdpAdminUI.STS.Identity.Configuration
{
    public class StsAuthentificationConfiguration
    {
        public class Google
        {
            public string ClientId { get; set; }
            public string Secret { get; set; }
        }

        public Google GoogleOptions { get; set; }
    }
}
