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
    
    public partial class UserCategoryMapping
    {
        public long Id { get; set; }
        public string Fk_MobileNumber { get; set; }
        public Nullable<long> CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual Category Category { get; set; }
        public virtual Mandi_UserInfo Mandi_UserInfo { get; set; }
        public virtual UserCategoryMapping UserCategoryMapping1 { get; set; }
        public virtual UserCategoryMapping UserCategoryMapping2 { get; set; }
    }
}
