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
    
    public partial class Periods
    {
        public System.Guid ID { get; set; }
        public Nullable<int> Period { get; set; }
        public string Short { get; set; }
        public string Name { get; set; }
        public string Starttime { get; set; }
        public string Endtime { get; set; }
        public int Status { get; set; }
        public Nullable<int> Sort { get; set; }
    }
}
