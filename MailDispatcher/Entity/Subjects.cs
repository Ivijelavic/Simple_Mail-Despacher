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
    
    public partial class Subjects
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public System.Guid ID { get; set; }
        public int Status { get; set; }
        public Nullable<System.Guid> ID_Smjerovi { get; set; }
        public Nullable<System.Guid> ID_SubjectDetail { get; set; }
    }
}