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

        public class DailyExpenseApiModel
        {
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public string UserId { get; set; }
        }

        [HttpPost("api/dailyexpense/add")]
        public IActionResult AddExpenseFromApi([FromBody] DailyExpenseApiModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verifica se l'ID utente esiste
            var user = _userManager.FindByIdAsync(model.UserId).Result;
            if (user == null)
            {
                return BadRequest("ID utente non valido.");
            }

            // Trova il viaggio in corso o il viaggio più recente
            var tripId = TrovaTripIdCorrenteORecente(model.UserId);

            if (tripId == 0)
            {
                return BadRequest("Nessun viaggio trovato per l'utente.");
            }

            var newExpense = new DailyExpense
            {
                Date = model.Date,
                Amount = decimal.Parse(model.Amount.ToString().Replace(',', '.')),
                ExpenseCategory = model.Category,
                Description = model.Description,
                UserId = model.UserId,
                InsertionDate = DateTime.Now,
                TripId = tripId
            };

            _context.DailyExpenses.Add(newExpense);
            _context.SaveChanges();

            return Ok(new { message = "Spesa aggiunta con successo" });
        }

        private int TrovaTripIdCorrenteORecente(string userId)
        {
            
            var tripCorrente = _context.Trips
                .Where(t => t.StartDate <= DateTime.Now && t.EndDate >= DateTime.Now)
                .OrderByDescending(t => t.EndDate)
                .FirstOrDefault();

            if (tripCorrente != null)
            {
                return tripCorrente.Id;
            }

            var tripRecente = _context.Trips
                .OrderByDescending(t => t.EndDate)
                .FirstOrDefault();

            if (tripRecente != null)
            {
                return tripRecente.Id;
            }

            return 0;
        }
    }
}