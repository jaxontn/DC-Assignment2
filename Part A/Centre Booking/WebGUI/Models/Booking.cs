namespace WebGUI.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public string CenterName { get; set; }
        public string BookingPersonName { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
    }
}
