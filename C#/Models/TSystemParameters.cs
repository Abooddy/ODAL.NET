using System;

namespace ODAL.Models
{
    public class TSystemParameters : IModel
    {
        public int? Id { get; set; }
        public string InstId { get; set; }
        public int? CreatedBy { get; set; }
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        public string ParameterDescription { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}