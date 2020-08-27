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
    
    public partial class Mandi_CartInfo
    {
        public long CartId { get; set; }
        public string CartType { get; set; }
        public Nullable<int> Fk_EnquiryId { get; set; }
        public Nullable<long> Fk_InterestedProductId { get; set; }
        public string Deal_Id { get; set; }
        public Nullable<long> Product_Id { get; set; }
        public string Product { get; set; }
        public string Quantity { get; set; }
        public string QuantityUnit { get; set; }
        public Nullable<int> Price { get; set; }
        public string ProductImage { get; set; }
        public string ProductAddress { get; set; }
        public string BuyerName { get; set; }
        public string BuyerNumber { get; set; }
        public string BuyerAddress { get; set; }
        public string SellerName { get; set; }
        public string Seller_MobileNumber { get; set; }
        public string SellerAddress { get; set; }
        public Nullable<decimal> Logistics_Cost { get; set; }
        public Nullable<decimal> Other_Charges { get; set; }
        public Nullable<bool> Status { get; set; }
    
        public virtual Mandi_InterestedProductForUser Mandi_InterestedProductForUser { get; set; }
        public virtual Mandi_UserEnquiry Mandi_UserEnquiry { get; set; }
    }
}
