using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remotely.Agent.Installer.Win.Models
{
    internal class LoginResult
    {
        public int oid { get; set; }
        public string serverURL { get; set; }
        public string organizationID { get; set; }
        public string dateCreated { get; set; }
        public string createdBy { get; set; }
        public string dateModified { get; set; }
        public string modifiedBy { get; set; }
        public bool isDeleted { get; set; }
    }
}
