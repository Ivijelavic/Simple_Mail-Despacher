//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MailDispatcher.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class ChangeManagementDetail
    {
        public System.Guid ID { get; set; }
        public System.Guid ID_CMaster { get; set; }
        public System.Guid ID_TeachersCM { get; set; }
        public System.Guid ID_Subject { get; set; }
        public System.Guid ID_Period { get; set; }
        public System.Guid ID_Classrom { get; set; }
        public Nullable<System.Guid> ID_Class { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.Guid> ID_Subject_CM { get; set; }
        public Nullable<System.DateTime> Datum { get; set; }
        public Nullable<int> Mail { get; set; }
        public Nullable<System.Guid> ID_StatusCM { get; set; }
        public Nullable<bool> Pay { get; set; }
        public string Poruka { get; set; }
        public Nullable<bool> ClanRV { get; set; }
    }
}
