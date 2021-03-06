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
    
    public partial class PictureProperty
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PictureProperty()
        {
            this.MenuItems = new HashSet<MenuItem>();
            this.OrganizationUnits = new HashSet<OrganizationUnit>();
            this.People = new HashSet<Person>();
            this.Banners = new HashSet<Banner>();
            this.Articles = new HashSet<Article>();
        }
    
        public int PicturePropertyID { get; set; }
        public int PictureMainID { get; set; }
        public string Filepath { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public Nullable<int> FileSize { get; set; }
        public Nullable<System.DateTime> Modified { get; set; }
        public bool IsDefault { get; set; }
        public bool IsThumbnail { get; set; }
        public bool IsOriginal { get; set; }
        public string Comment { get; set; }
        public Nullable<int> PersonID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MenuItem> MenuItems { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OrganizationUnit> OrganizationUnits { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Person> People { get; set; }
        public virtual Person Person { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Banner> Banners { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Article> Articles { get; set; }
    }
}
