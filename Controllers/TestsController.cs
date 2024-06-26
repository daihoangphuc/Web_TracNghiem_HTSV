using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Security.Claims;
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
    public class TestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // TestsController.cs

        public async Task<IActionResult> SelectTest()
        {
            var tests = await _context.Tests.ToListAsync(); // Lấy danh sách các bài test từ database
            return View(tests);
        }

        public async Task<IActionResult> TestDetails(string id, int page = 1)
        {
            var test = await _context.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Answers) // Include câu trả lời cho từng câu hỏi
                .FirstOrDefaultAsync(t => t.TestId == id);
            int PageSize = 1;
            if (test == null)
            {
                return NotFound();
            }

            var questions = test.Questions
            .OrderBy(q => q.QuestionId) // Sắp xếp theo Id hoặc thứ tự phù hợp
                .Skip((page - 1) * PageSize) // Bỏ qua số lượng phần tử đã phân trang
                .Take(PageSize) // Lấy số lượng câu hỏi cho từng trang
            .ToList();

            var paginatedList = new PaginatedList<Question>(questions, test.Questions.Count, page, PageSize);
            ViewBag.NextPage = page + 1;
            return View(paginatedList);
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MakeTestResult(string testId, string selectedAnswer)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Lấy UserId từ Claims (ví dụ: ASP.NET Core Identity)
                string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Lấy thông tin bài test từ database
                var test = await _context.Tests.FindAsync(testId);

                if (test == null)
                {
                    return NotFound();
                }

                // Tạo một bản ghi TestResult
                var testResult = new TestResult
                {
                    TestResultId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    TestId = testId,
                    SubmittedAt = DateTime.Now,
                    SelectedAnswer = selectedAnswer,
                    TotalScore = 10 // Giả sử mặc định điểm số
                };

                // Lưu TestResult vào database
                _context.Add(testResult);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Hoặc trả về một view khác tùy vào yêu cầu của bạn
            }
            else
            {
                // Nếu chưa đăng nhập thì chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }
        }



        // GET: Tests
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tests.ToListAsync());
        }

        // GET: Tests/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: Tests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TestId,TestName")] Test test)
        {

            _context.Add(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Tests/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            return View(test);
        }

        // POST: Tests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("TestId,TestName")] Test test)
        {
            if (id != test.TestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(test);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(test.TestId))
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
            return View(test);
        }

        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var test = await _context.Tests.FindAsync(id);
            if (test != null)
            {
                _context.Tests.Remove(test);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestExists(string id)
        {
            return _context.Tests.Any(e => e.TestId == id);
        }
    }
}
