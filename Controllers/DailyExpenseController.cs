using Microsoft.AspNetCore.Mvc;
using Business.Data;
using Business.Models;
using System;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Business.Controllers
{
    public class DailyExpenseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DailyExpenseController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Create(int tripId)
        {
            var viewModel = new DailyExpenseCreateViewModel
            {
                DailyExpense = new DailyExpense { Date = DateTime.Now },
                TripId = tripId
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DailyExpenseCreateViewModel viewModel, int tripId)
        {
            viewModel.DailyExpense.UserId = _userManager.GetUserId(User);
            viewModel.UserId = _userManager.GetUserId(User);
            viewModel.DailyExpense.InsertionDate = DateTime.Now;
            viewModel.DailyExpense.TripId = tripId;

            if (User.Identity.IsAuthenticated)
            {
                _context.DailyExpenses.Add(viewModel.DailyExpense);
                _context.SaveChanges();
                return RedirectToAction("TripSummary", "Trip");
            }
            else
            {
                ModelState.AddModelError("UserId", "L'utente deve essere autenticato.");
                return View(viewModel);
            }
        }

        public IActionResult Index(int tripId)
        {
            var expenses = _context.DailyExpenses.Where(e => e.TripId == tripId).ToList();
            ViewBag.TripId = tripId;
            return View(expenses);
        }

        public IActionResult Edit(int id)
        {
            var expense = _context.DailyExpenses.Find(id);
            if (expense == null)
            {
                return NotFound();
            }

            ViewBag.Categories = new SelectList(new[] { "Pranzo", "Cena", "Altro" });
            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DailyExpense expense)
        {
            if (ModelState.IsValid)
            {
                _context.Update(expense);
                _context.SaveChanges();
                return RedirectToAction("Index", new { tripId = expense.TripId });
            }

            // Ricrea la lista delle categorie in caso di errore
            ViewBag.Categories = new SelectList(new[] { "Pranzo", "Cena", "Altro" });

            return View(expense);
        }

        public IActionResult Delete(int id)
        {
            var expense = _context.DailyExpenses.Find(id);
            if (expense == null)
            {
                return NotFound();
            }
            return View(expense);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var expense = _context.DailyExpenses.Find(id);
            if (expense != null)
            {
                _context.DailyExpenses.Remove(expense);
                _context.SaveChanges();
                return RedirectToAction("Index", new { tripId = expense.TripId });
            }
            return NotFound();
        }
    }
}