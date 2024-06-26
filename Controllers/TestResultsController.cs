using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web_TracNghiem_HTSV.Data;
using Web_TracNghiem_HTSV.Models;

namespace Web_TracNghiem_HTSV.Controllers
{
    public class TestResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Result(string id)
        {

                if (id == null)
                {
                    return NotFound();
                }

                string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var test = await _context.Tests
                    .Include(t => t.Questions)
                        .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync(t => t.TestId == id);

                if (test == null)
                {
                    return NotFound("Bài kiểm tra không tồn tại hoặc bạn chưa làm bài này.");
                }

                var userTestResults = await _context.TestResults
                    .Where(tr => tr.TestId == id && tr.UserId == userId)
                    .Include(tr => tr.User)
                    .ToListAsync();

                ViewBag.UserId = userId;
                ViewBag.TestName = test.TestName;
                ViewBag.TotalScore = userTestResults.Sum(tr => tr.TotalScore);
                ViewBag.ListQuestion = test.Questions.ToList();
                ViewBag.ListTestResult = userTestResults;

                return View(test);
            }



            // GET: TestResults
            public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TestResults.Include(t => t.Test).Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TestResults/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testResult = await _context.TestResults
                .Include(t => t.Test)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TestResultId == id);
            if (testResult == null)
            {
                return NotFound();
            }

            return View(testResult);
        }

        // GET: TestResults/Create
        public IActionResult Create()
        {
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: TestResults/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TestResultId,UserId,TestId,SelectedAnswer,IsCorrect,SubmittedAt,TotalScore")] TestResult testResult)
        {
            if (ModelState.IsValid)
            {
                _context.Add(testResult);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", testResult.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", testResult.UserId);
            return View(testResult);
        }

        // GET: TestResults/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult == null)
            {
                return NotFound();
            }
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", testResult.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", testResult.UserId);
            return View(testResult);
        }

        // POST: TestResults/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TestResultId,UserId,TestId,SelectedAnswer,IsCorrect,SubmittedAt,TotalScore")] TestResult testResult)
        {
            if (id != testResult.TestResultId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(testResult);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestResultExists(testResult.TestResultId))
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
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", testResult.TestId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", testResult.UserId);
            return View(testResult);
        }

        // GET: TestResults/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var testResult = await _context.TestResults
                .Include(t => t.Test)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TestResultId == id);
            if (testResult == null)
            {
                return NotFound();
            }

            return View(testResult);
        }

        // POST: TestResults/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult != null)
            {
                _context.TestResults.Remove(testResult);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestResultExists(string id)
        {
            return _context.TestResults.Any(e => e.TestResultId == id);
        }
    }
}
