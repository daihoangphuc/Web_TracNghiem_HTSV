using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web_TracNghiem_HTSV.Services;
using Web_TracNghiem_HTSV.Data;
using Web_TracNghiem_HTSV.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Web_TracNghiem_HTSV.Controllers
{
    [Authorize]
    public class QuestionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public QuestionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> MakeTestResult(string questionId, string selectedAnswer)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Lấy UserId từ Claims (ví dụ: ASP.NET Core Identity)
                string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                // Lấy thông tin câu hỏi dựa trên questionId
                var question = await _context.Questions.FindAsync(questionId);

                if (question == null)
                {
                    return NotFound();
                }

                // Tạo một bản ghi TestResult
                var testResult = new TestResult
                {
                    TestResultId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    QuestionId = questionId,
                    SubmittedAt = DateTime.Now,
                    SelectedAnswer = selectedAnswer,
                    TotalScore = 10 // Mặc định điểm số
                };
                // Lưu TestResult vào database
                _context.Add(testResult);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Nếu chưa đăng nhập thì chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }
            return RedirectToAction(nameof(Index)); // Hoặc trả về một view khác tùy vào yêu cầu của bạn
        }

        // GET: Questions
        public async Task<IActionResult> Index(int page=1)
        {
            /*return View(await _context.Questions.ToListAsync());*/
            var questions = _context.Questions.Include(q => q.Answers).Skip((page - 1) * 1).Take(1).ToList();
            var paginatedList = new PaginatedList<Question>(questions, _context.Questions.Count(), page, 1);
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
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionId,QuestionContent,CorrectAnswer")] Question question)
        {
            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
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
            return View(question);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("QuestionId,QuestionContent,CorrectAnswer")] Question question)
        {
            if (id != question.QuestionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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
            }
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
