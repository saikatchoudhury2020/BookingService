using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models
{
    public class UserMaster
    {
        public int PersonID { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string DisplayName { get; set; }
        public string OrganizationUnitName { get; set; }
        public string OrganizationUnitId { get; set; }
        public bool IsPrimary { get; set; }

    }

    public class CompanyMaster
    {
        public string OrganizationUnitName { get; set; }
        public string OrganizationUnitId { get; set; }
        public bool IsPrimary { get; set; }

        public bool IsMVA { get; set; }
    }

}