//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GrowIndigo.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Crop_Master
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Crop_Master()
        {
            this.Mandi_ProductMaster = new HashSet<Mandi_ProductMaster>();
            this.Mandi_ProductMaster1 = new HashSet<Mandi_ProductMaster>();
            this.Variety_Master = new HashSet<Variety_Master>();
        }
    
        public int CropId { get; set; }
        public string CropName { get; set; }
        public string CategoryName { get; set; }
        public string CategoryId { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual Crop_Master Crop_Master1 { get; set; }
        public virtual Crop_Master Crop_Master2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Mandi_ProductMaster> Mandi_ProductMaster { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Mandi_ProductMaster> Mandi_ProductMaster1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Variety_Master> Variety_Master { get; set; }
    }
}
