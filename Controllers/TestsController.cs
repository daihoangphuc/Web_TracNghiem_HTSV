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
using Web_TracNghiem_HTSV.Services;

namespace Web_TracNghiem_HTSV.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Administrators")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockTest(string id)
        {
            var test = await _context.Tests.FindAsync(id);

            if (test == null)
            {
                return NotFound();
            }

            test.IsLocked = true; // Mở khóa bài test

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

        [Authorize(Roles = "Administrators")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockTest(string id)
        {
            var test = await _context.Tests.FindAsync(id);

            if (test == null)
            {
                return NotFound();
            }

            test.IsLocked = false; // Mở khóa bài test

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


        [Authorize]
        [Authorize(Roles = "Administrators")]

        public ActionResult ListTests()
        {
            // Lấy danh sách các bài Test
            var tests = _context.Tests.Include(t => t.TestResults).ToList(); // db là đối tượng DbContext của bạn

            // Lấy UserId của người dùng hiện tại (cần phải có xác thực người dùng)
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Phương thức này tùy thuộc vào cách bạn xác thực người dùng

            // Chuẩn bị danh sách các ViewModel để truyền vào view
            List<TestViewModel> testViewModels = new List<TestViewModel>();

            foreach (var test in tests)
            {
                // Kiểm tra xem người dùng đã làm bài Test này chưa
                var testResult = test.TestResults.FirstOrDefault(tr => tr.UserId == userId);

                // Tạo ViewModel cho từng Test để truyền vào view
                var viewModel = new TestViewModel
                {
                    TestId = test.TestId,
                    TestName = test.TestName,
                    IsTestTaken = testResult != null
                };

                testViewModels.Add(viewModel);
            }

            return View(testViewModels);
        }


        [Authorize]
        public async Task<IActionResult> ResultOfTest(string id)
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
            ViewBag.TotalScore = userTestResults.Where(u => u.UserId == userId).Sum(tr => tr.TotalScore);
            ViewBag.ListQuestion = test.Questions.ToList();
            ViewBag.ListTestResult = userTestResults;



            var userTestResults1 = await _context.Tests
              .Include(tr => tr.TestResults)
              .Include(tr => tr.Questions)
                    .ThenInclude(q => q.Answers)
              .ToListAsync();


            ViewBag.UserTestResult = userTestResults1;
            return View(test);
        }

        // TestsController.cs

        [Authorize]
        public async Task<IActionResult> SelectTest()
        {
            var tests = await _context.Tests.ToListAsync(); // Lấy danh sách các bài test từ database
            return View(tests);
        }

        [Authorize]
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
        public async Task<IActionResult> MakeTestResult(string testId, string questionId, string selectedAnswer)
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

                // Lấy đáp án đúng cho câu hỏi hiện tại
                var correctAnswer = await _context.Questions
                                                 .Where(q => q.QuestionId == questionId)
                                                 .Select(q => q.CorrectAnswer)
                                                 .FirstOrDefaultAsync();



                if (correctAnswer == null)
                {
                    return NotFound("Question not found.");
                }

                // Kiểm tra đáp án đã chọn có đúng không
                bool isCorrect = correctAnswer == selectedAnswer;

                // Tạo một bản ghi TestResult
                var testResult = new TestResult
                {
                    TestResultId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    TestId = testId,
                    SubmittedAt = DateTime.Now,
                    IsCorrect = isCorrect,
                    QuestionId = questionId,
                    SelectedAnswer = selectedAnswer,
                    TotalScore = isCorrect ? 10 : 0 // Giả sử điểm số mặc định là 10 nếu đúng
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
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var tests = await _context.Tests.ToListAsync();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy userId của người dùng hiện tại

            // Lấy danh sách các TestResult của người dùng hiện tại
            var userTestResults = await _context.TestResults
                .Where(tr => tr.UserId == userId)
                .Select(tr => tr.TestId)
                .ToListAsync();
            var lisquestion = await _context.Questions.Select(tr => tr.TestId).ToListAsync();

            var allUserTestResults = await _context.TestResults
            .GroupBy(tr => tr.TestId)
            .Select(g => new { TestId = g.Key, UserCount = g.Select(tr => tr.UserId).Distinct().Count() })
            .ToListAsync();

            ViewBag.countUserMakeTest = allUserTestResults;
            ViewBag.ListQuestion = lisquestion;
            ViewBag.UserTestResulted = userTestResults; // Lưu danh sách TestId mà người dùng đã làm vào ViewBag

            return View(tests);
           /* return View(await _context.Tests.ToListAsync());*/
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
        [Authorize(Roles = "Administrators")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Administrators")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TestId,TestName,IsLocked")] Test test)
        {

                test.TestId = Guid.NewGuid().ToString();
                _context.Add(test);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
          /*  return View(test);*/
        }

        // GET: Tests/Edit/5
        [Authorize(Roles = "Administrators")]
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
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> Edit(string id, [Bind("TestId,TestName,IsLocked")] Test test)
        {
            if (id != test.TestId)
            {
                return NotFound();
            }


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

            return View(test);
        }

        // GET: Tests/Delete/5
        [Authorize(Roles = "Administrators")]
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
        [Authorize(Roles = "Administrators")]
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
