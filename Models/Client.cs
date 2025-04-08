using System;
using System.Collections.Generic;
using Business.Models;

namespace Business.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
        public string Luogo { get; set; }
        public DateTime DataCreazione { get; set; }

        // (Opzionale) Proprietà di navigazione se hai relazioni con altre entità
        public ICollection<Trip>? Trips { get; set; } // Un cliente può avere molte trasferte
    }
}