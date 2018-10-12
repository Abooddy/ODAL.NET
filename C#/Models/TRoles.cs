using System;

namespace ODAL.Models
{
    public class TRoles : IModel
    {
        public int? Id { get; set; }
        public int? CreatedBy { get; set; }
        public string RoleName { get; set; }
        public string Claims { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}