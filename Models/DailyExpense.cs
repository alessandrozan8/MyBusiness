using System;

namespace Business.Models
{
    public class DailyExpense
    {
        public int Id { get; set; }
        public int TripId { get; set; } // Chiave esterna per Trip
        public string UserId { get; set; } // Chiave esterna per l'utente (da AspNetUsers)
        public DateTime Date { get; set; }
        public string ExpenseCategory { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public DateTime InsertionDate { get; set; }
    }
}