namespace ShoeWebStore.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderDate { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } // ФИО клиента для вывода
    }
}