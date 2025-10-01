using SmartSchoolAPI.Entities; 
using SmartSchoolAPI.Enums;
using System;

namespace SmartSchoolAPI.DTOs.Public
{
    public class RegistrationStatusDto
    {
        public int RegistrationId { get; set; }
        public string FullName { get; set; }
        public string ProgramName { get; set; }
        public RegistrationStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
        public PaymentSettings? PaymentDetails { get; set; } 
        public string? AdminNotes { get; set; }
    }
}