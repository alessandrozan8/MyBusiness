using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Business.Models;
using System.Collections.Generic;
using System.Linq;
using Business.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System;

[Authorize]
public class TripController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public TripController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult TripSummary()
    {
        ViewData["Title"] = "Riepilogo Trasferte";

        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            ViewBag.TotalDays = 0;
            ViewBag.TotalExpense = 0m;
            ViewBag.TotalBudget = 0m;
            ViewBag.AverageExpensePerDay = 0m;
            ViewBag.TotalAmountNotSpent = 0m;
            return View(new List<TripSummaryViewModel>());
        }

        var originalTrips = _context.Trips
            .Include(t => t.Client)
            .Where(t => t.RequestingUserId == userId)
            .ToList(); // Recupera le trasferte in memoria

        var tripSummaryList = originalTrips.Select(t => new TripSummaryViewModel
        {
            NomeCliente = t.Client.Nome,
            UserId = t.RequestingUserId,
            LuogoTrasferta = t.Client.Luogo,
            InizioTrasferta = Convert.ToString(t.StartDate),
            FineTrasferta = Convert.ToString(t.EndDate),
            NumeroGiorniTrasferta = (int)((t.EndDate.HasValue && t.StartDate.HasValue) ? (t.EndDate.Value - t.StartDate.Value).TotalDays : 0),
            BudgetTotalePerGiorno = (t.BudgetPranzo ?? 0m) + (t.DinnerBudget ?? 0m),
            BudgetTotaleTrasferta = (int)((t.EndDate.HasValue && t.StartDate.HasValue) ? (t.EndDate.Value - t.StartDate.Value).TotalDays : 0) * ((t.BudgetPranzo ?? 0m) + (t.DinnerBudget ?? 0m)),
            SpesaTotaleEffettiva = (_context.DailyExpenses
                .Where(de => de.TripId == t.Id)
                .Sum(de => de.Amount) as decimal?) ?? 0m,
            TripId = t.Id,
            IsIncluded = true
        })
        .OrderByDescending(ts => ts.FineTrasferta)
        .ToList();

        var includedItems = tripSummaryList.Where(m => m.IsIncluded);
        ViewBag.TotalDays = includedItems.Sum(m => m.NumeroGiorniTrasferta);
        ViewBag.TotalExpense = includedItems.Sum(m => m.SpesaTotaleEffettiva);
        ViewBag.TotalBudget = includedItems.Sum(m => m.BudgetTotaleTrasferta);
        ViewBag.AverageExpensePerDay = ViewBag.TotalDays > 0 ? ViewBag.TotalExpense / ViewBag.TotalDays : 0;
        ViewBag.TotalAmountNotSpent = ViewBag.TotalBudget - ViewBag.TotalExpense;

        return View(tripSummaryList);
    }

    [HttpPost]
    public IActionResult UpdateTripSummary(List<TripSummaryViewModel> model)
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            ViewBag.TotalDays = 0;
            ViewBag.TotalExpense = 0m;
            ViewBag.TotalBudget = 0m;
            ViewBag.AverageExpensePerDay = 0m;
            ViewBag.TotalAmountNotSpent = 0m;
            return View("TripSummary", new List<TripSummaryViewModel>());
        }

        var originalTrips = _context.Trips
            .Include(t => t.Client)
            .Where(t => t.RequestingUserId == userId)
            .Select(t => new TripSummaryViewModel
            {
                NomeCliente = t.Client.Nome,
                UserId = t.RequestingUserId,
                LuogoTrasferta = t.Client.Luogo,
                InizioTrasferta = Convert.ToString(t.StartDate),
                FineTrasferta = Convert.ToString(t.EndDate),
                NumeroGiorniTrasferta = (int)((t.EndDate.HasValue && t.StartDate.HasValue) ? (t.EndDate.Value - t.StartDate.Value).TotalDays : 0),
                BudgetTotalePerGiorno = (t.BudgetPranzo ?? 0m) + (t.DinnerBudget ?? 0m),
                BudgetTotaleTrasferta = (int)((t.EndDate.HasValue && t.StartDate.HasValue) ? (t.EndDate.Value - t.StartDate.Value).TotalDays : 0) * ((t.BudgetPranzo ?? 0m) + (t.DinnerBudget ?? 0m)),
                SpesaTotaleEffettiva = (_context.DailyExpenses
                    .Where(de => de.TripId == t.Id)
                    .Sum(de => de.Amount) as decimal?) ?? 0m,
                TripId = t.Id,
                IsIncluded = true
            })
            .OrderByDescending(ts => ts.FineTrasferta)
            .ToList();

        for (int i = 0; i < originalTrips.Count; i++)
        {
            var updatedTrip = model.FirstOrDefault(t => t.TripId == originalTrips[i].TripId);
            if (updatedTrip != null)
            {
                originalTrips[i].IsIncluded = updatedTrip.IsIncluded;
            }
        }

        var includedItems = originalTrips.Where(m => m.IsIncluded);
        ViewBag.TotalDays = includedItems.Sum(m => m.NumeroGiorniTrasferta);
        ViewBag.TotalExpense = includedItems.Sum(m => m.SpesaTotaleEffettiva);
        ViewBag.TotalBudget = includedItems.Sum(m => m.BudgetTotaleTrasferta);
        ViewBag.AverageExpensePerDay = ViewBag.TotalDays > 0 ? ViewBag.TotalExpense / ViewBag.TotalDays : 0;
        ViewBag.TotalAmountNotSpent = ViewBag.TotalBudget - ViewBag.TotalExpense;

        return View("TripSummary", originalTrips);
    }

    public IActionResult Create()
    {
        var clients = _context.Clients.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nome + " - " + c.Luogo
        }).ToList();

        var viewModel = new TripCreateViewModel
        {
            Trip = new Trip
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                BudgetPranzo = 20.00m,
                DinnerBudget = 45.00m
            },
            ClientList = clients
        };

        var tripList = _context.Trips.Include(t => t.Client)
            .OrderByDescending(t => t.EndDate)
            .ToList();
        ViewBag.TripList = tripList;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TripCreateViewModel viewModel)
    {
        viewModel.Trip.CreationDate = DateTime.Now;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            viewModel.Trip.RequestingUserId = userId;
        }

        _context.Trips.Add(viewModel.Trip);
        try
        {
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Si è verificato un errore durante il salvataggio della trasferta.");
            System.Diagnostics.Debug.WriteLine(ex.ToString());
        }

        var clients = _context.Clients.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Nome
        }).ToList();
        viewModel.ClientList = clients;

        var tripList = _context.Trips.Include(t => t.Client)
            .OrderByDescending(t => t.EndDate)
            .ToList();
        ViewBag.TripList = tripList;
        ViewBag.ClientList = tripList;

        return View(viewModel);
    }

    public IActionResult Index()
    {
        var trips = _context.Trips.Include(t => t.Client).ToList();
        return View(trips);
    }

    public TripSummaryViewModel GetTripSummary(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return new TripSummaryViewModel();
        }

        var trips = _context.Trips
            .Where(t => t.RequestingUserId == userId)
            .ToList();

        decimal totalAmountNotSpent = 0;

        foreach (var trip in trips)
        {
            var totalBudget = (int)((trip.EndDate.HasValue && trip.StartDate.HasValue) ? (trip.EndDate.Value - trip.StartDate.Value).TotalDays : 0) * ((trip.BudgetPranzo ?? 0m) + (trip.DinnerBudget ?? 0m));

            var totalExpense = _context.DailyExpenses
                .Where(de => de.TripId == trip.Id)
                .Sum(de => de.Amount);

            totalAmountNotSpent += totalBudget - totalExpense;
        }

        return new TripSummaryViewModel
        {
            TotalAmountNotSpent = totalAmountNotSpent
        };
    }
}