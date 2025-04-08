namespace Business.Models
{


    public class TripSummaryViewModel
    {
        public string NomeCliente { get; set; }
        public string UserId { get; set; }
        public string LuogoTrasferta { get; set; }
        public string InizioTrasferta { get; set; }
        public string FineTrasferta { get; set; }
        public int NumeroGiorniTrasferta { get; set; }
        public decimal BudgetTotalePerGiorno { get; set; }
        public decimal BudgetTotaleTrasferta { get; set; }
        public decimal SpesaTotaleEffettiva { get; set; }
        public int TripId { get; set; }
        public bool IsIncluded { get; set; } = true;
        public decimal TotalAmountNotSpent { get; set; }
    }

}