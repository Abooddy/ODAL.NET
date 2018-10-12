using System;

namespace ODAL.Models
{
    public class TUsers : IModel
    {
        public int? Id { get; set; }
        public string InstId { get; set; }
        public int? RegisteredBy { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string IsBlocked { get; set; }
        public int? FailedLoginCounter { get; set; }
        public DateTime? InsertDate { get; set; }
        public DateTime? BlockDate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserRole { get; set; }
    }
}