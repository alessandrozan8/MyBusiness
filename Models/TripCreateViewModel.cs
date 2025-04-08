using Business.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Business.Models
{
    public class TripCreateViewModel
    {
        public Trip Trip { get; set; }
        public List<SelectListItem> ClientList { get; set; }
    }
}
