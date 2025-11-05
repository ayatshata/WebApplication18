namespace MughtaribatHouse.Models.DTOs
{
    public class ResidentDto1
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public bool IsActive { get; set; }
        public string Notes { get; set; }
    }

    public class CreateResidentDto1
    {
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckInDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Notes { get; set; }
    }

    public class UpdateResidentDto1
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Notes { get; set; }
    }
}