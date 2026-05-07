using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FR_WS2_BaseLab.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FR_WS2_BaseLab.Controllers;

public class TopicsController : Controller
{
    private readonly FrWs2BaselabContext _context;

    public TopicsController(FrWs2BaselabContext context)
    {
        _context = context;
    }

    // GET: Topics
    public async Task<IActionResult> Index(int? id)
    {
        ViewData["CategoryId"] = id;
        var frWs2BaselabContext = _context.Topics.Where(t=>t.Id == id);
        return View(await frWs2BaselabContext.ToListAsync());
    }

    // GET: Topics/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics
            .Include(t => t.Cat)
            .Include(t => t.User)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (topic == null)
        {
            return NotFound();
        }

        return View(topic);
    }

    // GET: Topics/Create
    [Authorize]
    public IActionResult Create(int? id)
    {
        ViewData["CategoryId"] = id;
        return View();
    }

    // POST: Topics/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create([Bind("CatId,UserId,Inactive,Title,Texte,Date,Views")] Topic topic)
    {
        if (ModelState.IsValid)
        {
            topic.Date = DateTime.Now; 
            topic.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);  
            _context.Add(topic);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { id = topic.CatId});
        }
        ViewData["CatId"] = topic.CatId;
        return View(topic);
    }

    // GET: Topics/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics.FindAsync(id);
        if (topic == null)
        {
            return NotFound();
        }
        ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id", topic.CatId);
        ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", topic.UserId);
        return View(topic);
    }

    // POST: Topics/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CatId,UserId,Inactive,Title,Texte,Date,Views")] Topic topic)
    {
        if (id != topic.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(topic);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TopicExists(topic.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["CatId"] = new SelectList(_context.Categories, "Id", "Id", topic.CatId);
        ViewData["UserId"] = new SelectList(_context.AspNetUsers, "Id", "Id", topic.UserId);
        return View(topic);
    }

    // GET: Topics/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var topic = await _context.Topics
            .Include(t => t.Cat)
            .Include(t => t.User)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (topic == null)
        {
            return NotFound();
        }

        return View(topic);
    }

    // POST: Topics/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var topic = await _context.Topics.FindAsync(id);
        if (topic != null)
        {
            _context.Topics.Remove(topic);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TopicExists(int id)
    {
        return _context.Topics.Any(e => e.Id == id);
    }
}
