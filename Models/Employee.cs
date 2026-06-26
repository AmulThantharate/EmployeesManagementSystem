using System.ComponentModel;

namespace EmployeesManagementSystem.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;

        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;

        [DisplayName("Position")]
        public string Position { get; set; } = string.Empty;

        [DisplayName("Date of Birth")]
        public DateTime DateOfBirth { get; set; }


        [DisplayName("Country Name")]
        public string Country { get; set; } = string.Empty;


        [DisplayName("Email Address")]
        public string EmailAddress { get; set; } = string.Empty;

        [DisplayName("Addres Name")]
        public string Address { get; set; } = string.Empty;


        [DisplayName("City Name")]
        public string City { get; set; } = string.Empty;


        [DisplayName("Postal Code")]
        public string PostalCode { get; set; } = string.Empty;


        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;


        [DisplayName("Created By")]
        public string CreatedById { get; set; } = string.Empty;


        [DisplayName("Created On")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
