using Business.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Business.Models // Sostituisci YourProjectName
{
    public class DailyExpenseCreateViewModel
    {
        public DailyExpense DailyExpense { get; set; }
        public int TripId { get; set; }
        public string UserId { get; set; }
    }
}