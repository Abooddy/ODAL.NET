using System;

namespace ODAL.Models
{
    public class TInstitutions : IModel
    {
        public int? Id { get; set; }
        public string InstId { get; set; }
        public string InstFullName { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}