using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web_TracNghiem_HTSV.Data;
using Web_TracNghiem_HTSV.Models;
using Web_TracNghiem_HTSV.Services;

namespace Web_TracNghiem_HTSV.Controllers
{
    [Authorize(Roles = "Administrators")]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Questions
        public async Task<IActionResult> Index(string searchString, int pageIndex = 1, int pageSize = 5)
        {
            // Lấy tất cả các câu hỏi, bao gồm cả bài kiểm tra liên quan
            var questions = from q in _context.Questions.Include(q => q.Test)
                            select q;

            // Thực hiện tìm kiếm nếu có searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                questions = questions.Where(q => q.QuestionContent.Contains(searchString));
            }

            // Phân trang
            var paginatedList = await PaginatedList<Question>.CreateAsync(questions.AsNoTracking(), pageIndex, pageSize);

            // Truyền searchString vào ViewBag để giữ lại giá trị tìm kiếm khi phân trang
            ViewBag.CurrentFilter = searchString;

            return View(paginatedList);
        }

        // GET: Questions/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Test)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: Questions/Create
        public IActionResult Create()
        {
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestName");
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionId,QuestionContent,CorrectAnswer,TestId")] Question question)
        {

                question.QuestionId = Guid.NewGuid().ToString();
                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestName", question.TestId);
            return View(question);
        }

        // GET: Questions/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", question.TestId);
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("QuestionId,QuestionContent,CorrectAnswer,TestId")] Question question)
        {
            if (id != question.QuestionId)
            {
                return NotFound();
            }

                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.QuestionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", question.TestId);
            return View(question);
        }

        // GET: Questions/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Test)
                .FirstOrDefaultAsync(m => m.QuestionId == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(string id)
        {
            return _context.Questions.Any(e => e.QuestionId == id);
        }
    }
}
