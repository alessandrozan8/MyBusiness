using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Business.Data;
using Business.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Business.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);

                // Recupera le ultime spese dell'utente
                var lastExpenses = _context.DailyExpenses
                    .Where(e => e.UserId == userId)
                    .OrderByDescending(e => e.InsertionDate)
                    .Take(6)
                    .ToList();

                ViewBag.LastExpenses = lastExpenses;

                // Calcola il totale non speso per le trasferte
                var trips = _context.Trips
                    .Where(t => t.RequestingUserId == userId)
                    .ToList();

                decimal totalAmountNotSpent = 0;
                foreach (var trip in trips)
                {
                    decimal totalBudget = (int)((trip.EndDate.HasValue && trip.StartDate.HasValue) ? (trip.EndDate.Value - trip.StartDate.Value).TotalDays : 0) *
                                          ((trip.BudgetPranzo ?? 0m) + (trip.DinnerBudget ?? 0m));

                    var dailyExpenses = _context.DailyExpenses
                        .Where(e => e.TripId == trip.Id)
                        .ToList();

                    decimal totalExpenses = dailyExpenses.Sum(e => e.Amount);

                    totalAmountNotSpent += totalBudget - totalExpenses;
                }

                ViewBag.TotalAmountNotSpent = totalAmountNotSpent;

                // Recupera il numero totale di trasferte
                ViewBag.TotalTrips = _context.Trips
                    .Where(t => t.RequestingUserId == userId)
                    .Count();

                // Calcola il budget totale di tutte le trasferte (calcolo in memoria)
                decimal totalBudgetSum = 0;
                foreach (var trip in trips)
                {
                    totalBudgetSum += (int)((trip.EndDate.HasValue && trip.StartDate.HasValue) ? (trip.EndDate.Value - trip.StartDate.Value).TotalDays : 0) *
                                       ((trip.BudgetPranzo ?? 0m) + (trip.DinnerBudget ?? 0m));
                }

                ViewBag.TotalBudget = totalBudgetSum;

                // Recupera l'ultima trasferta
                var lastTrip = _context.Trips
                    .Include(t => t.Client)
                    .Where(t => t.RequestingUserId == userId)
                    .OrderByDescending(t => t.EndDate)
                    .FirstOrDefault();

                ViewBag.LastTrip = lastTrip;

                // Calcola il totale delle spese per l'ultima trasferta
                ViewBag.LastTripExpenses = lastTrip != null ?
                    _context.DailyExpenses.Where(e => e.TripId == lastTrip.Id).Sum(e => e.Amount) :
                    0m;

                return View();
            }
            else
            {
                return View();
            }
        }
    }
}