using Microsoft.AspNetCore.Mvc;
using Business.Models;
using Business.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[Authorize]
public class ClientController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Create()
    {
        var clientList = _context.Clients.ToList();

        ViewBag.ClientList = clientList;

        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (ModelState.IsValid)
        {
            client.DataCreazione = DateTime.Now; // Imposta la data di creazione corrente
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // Se il modello non è valido, recupera comunque la lista dei clienti
        var clientList = _context.Clients.ToList();
        ViewBag.ClientList = clientList;

        return View(client);
    }

    // (Opzionale) Azione per mostrare un elenco di clienti
    public IActionResult Index()
    {
        var clients = _context.Clients.ToList();
        return View(clients);
    }
}