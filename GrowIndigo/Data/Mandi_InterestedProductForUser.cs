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
    
    public partial class Mandi_InterestedProductForUser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Mandi_InterestedProductForUser()
        {
            this.Mandi_CartInfo = new HashSet<Mandi_CartInfo>();
        }
    
        public long Id { get; set; }
        public string Fk_MobileNumber { get; set; }
        public string BuyerId { get; set; }
        public string BuyerAddress { get; set; }
        public string ProductId { get; set; }
        public string CropName { get; set; }
        public string VarietyName { get; set; }
        public string Quantity { get; set; }
        public string QualitySpecification { get; set; }
        public string DeliveryLocation { get; set; }
        public string ExpectedPrice { get; set; }
        public string IsPriceNegotiable { get; set; }
        public string Remarks { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Mandi_CartInfo> Mandi_CartInfo { get; set; }
        public virtual Mandi_InterestedProductForUser Mandi_InterestedProductForUser1 { get; set; }
        public virtual Mandi_InterestedProductForUser Mandi_InterestedProductForUser2 { get; set; }
        public virtual Mandi_UserInfo Mandi_UserInfo { get; set; }
    }
}
