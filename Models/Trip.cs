using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Business.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Business.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ClienteId { get; set; } // Chiave esterna per Client
        public decimal? BudgetPranzo { get; set; }
        public decimal? DinnerBudget { get; set; }
        public string RequestingUserId { get; set; } // Chiave esterna per l'utente richiedente (da AspNetUsers)
        public IdentityUser? RequestingUser { get; set; }
        public DateTime CreationDate { get; set; }

        // Proprietà di navigazione
        public Client? Client { get; set; }
        // public ApplicationUser RequestingUser { get; set; } // Assicurati che ApplicationUser sia definito correttamente
        public ICollection<DailyExpense>? DailyExpenses { get; set; } // Una trasferta può avere molte spese giornaliere
    }
}