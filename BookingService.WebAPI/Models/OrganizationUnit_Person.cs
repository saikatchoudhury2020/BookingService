//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BookingService.WebAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class OrganizationUnit_Person
    {
        public int ObjectID { get; set; }
        public int PersonID { get; set; }
        public int OrganizationUnitID { get; set; }
        public int AssociationID { get; set; }
        public int Priority { get; set; }
        public System.DateTime Modified { get; set; }
    
        public virtual OrganizationUnit OrganizationUnit { get; set; }
        public virtual Person Person { get; set; }
    }
}