using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Business.Data;
using Business.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

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
                decimal totalBudgetSum = 0;
                foreach (var trip in trips)
                {
                    decimal totalBudget = (int)((trip.EndDate.HasValue && trip.StartDate.HasValue) ? (trip.EndDate.Value - trip.StartDate.Value).TotalDays : 0) *
                                          ((trip.BudgetPranzo ?? 0m) + (trip.DinnerBudget ?? 0m));

                    var dailyExpenses = _context.DailyExpenses
                        .Where(e => e.TripId == trip.Id)
                        .ToList();

                    decimal totalExpenses = dailyExpenses.Sum(e => e.Amount);

                    totalAmountNotSpent += totalBudget - totalExpenses;
                    totalBudgetSum += totalBudget;
                }

                ViewBag.TotalAmountNotSpent = totalAmountNotSpent;

                // Recupera il numero totale di trasferte
                ViewBag.TotalTrips = _context.Trips
                    .Where(t => t.RequestingUserId == userId)
                    .Count();

                // Calcola il budget totale di tutte le trasferte (calcolo in memoria)
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

                // Calcola il budget residuo giornaliero per l'ultima trasferta
                if (lastTrip != null)
                {
                    var tripId = lastTrip.Id;
                    var startDate = lastTrip.StartDate;
                    var endDate = lastTrip.EndDate;

                    var dailyBudgetLabels = new List<string>();
                    var dailyBudgetValues = new List<decimal>();

                    decimal totalTripBudget = (int)((lastTrip.EndDate.HasValue && lastTrip.StartDate.HasValue) ? (lastTrip.EndDate.Value - lastTrip.StartDate.Value).TotalDays : 0) *
                                              ((lastTrip.BudgetPranzo ?? 0m) + (lastTrip.DinnerBudget ?? 0m));

                    // Aggiungi il budget totale come primo valore (Initial Budget)
                    dailyBudgetLabels.Add("Initial Budget");
                    dailyBudgetValues.Add(totalTripBudget);

                    decimal remainingBudget = totalTripBudget; // Budget totale iniziale

                    for (DateTime date = startDate.Value; date <= endDate.Value; date = date.AddDays(1))
                    {
                        dailyBudgetLabels.Add(date.ToShortDateString());

                        // Calcola le spese per il giorno corrente
                        decimal dailyExpenses = _context.DailyExpenses
                            .Where(e => e.TripId == tripId && e.Date == date)
                            .Sum(e => e.Amount);

                        remainingBudget -= dailyExpenses;
                        dailyBudgetValues.Add(remainingBudget);
                    }

                    ViewBag.DailyBudgetLabels = dailyBudgetLabels;
                    ViewBag.DailyBudgetValues = dailyBudgetValues;

                    // Calcola il budget totale della trasferta
                    ViewBag.TotalTripBudget = totalTripBudget;

                    // Calcola il budget rimanente
                    ViewBag.RemainingTripBudget = dailyBudgetValues.Last();

                    // Calcola la spesa media giornaliera
                    ViewBag.AverageDailyExpense = (decimal)(endDate.Value - startDate.Value).TotalDays > 0 ?
                        _context.DailyExpenses.Where(e => e.TripId == tripId).Sum(e => e.Amount) / (decimal)(DateTime.Now - startDate.Value).TotalDays :
                        0m;
                }

                return View();
            }
            else
            {
                return View();
            }
        }
    }
}