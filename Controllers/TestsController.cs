﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
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
        public async Task<IActionResult> MoreDetailOfTest(string id)
        {
            // Giải mã `TestId` từ Base64
            var deCodeId = EncodeUrl.Decode(id);

            if (string.IsNullOrEmpty(deCodeId))
            {
                return NotFound();
            }

            // Lấy dữ liệu từ cơ sở dữ liệu
            var resultDetails = await _context.TestResults
                .Where(tr => tr.TestId == deCodeId)
                .Join(
                    _context.Users,
                    tr => tr.UserId,
                    u => u.Id,
                    (tr, u) => new { tr, u }
                )
                .GroupBy(t => new { t.tr.UserId, t.u.UserName, t.tr.TestId })
                .Select(g => new
                {
                    UserId = g.Key.UserId,
                    UserName = g.Key.UserName,
                    TestId = g.Key.TestId,
                    TotalScore = g.Sum(t => t.tr.TotalScore),
                    TimeTaken = g.Sum(t => EF.Functions.DateDiffSecond(t.tr.StartedAt, t.tr.SubmittedAt)),
                    LatestSubmittedAt = g.Max(t => t.tr.SubmittedAt)
                })
                .ToListAsync();

            // Chuyển đổi dữ liệu và sắp xếp trong bộ nhớ
            var resultDetailsViewModel = resultDetails
                .Select(r => new UserTestResultDetailViewModel
                {
                    UserId = r.UserId,
                    UserName = r.UserName,
                    TestId = r.TestId,
                    TotalScore = r.TotalScore,
                    TimeTaken = TimeSpan.FromSeconds(r.TimeTaken),
                    LatestSubmittedAt = r.LatestSubmittedAt
                })
                .OrderByDescending(r => r.TotalScore) // Sắp xếp theo điểm số từ cao đến thấp
                .ThenBy(r => r.TimeTaken) // Nếu điểm số bằng nhau, sắp xếp theo thời gian làm bài từ thấp đến cao
                .ToList();

            if (resultDetailsViewModel == null || !resultDetailsViewModel.Any())
            {
                return NotFound("Không tìm thấy kết quả làm bài cho bài kiểm tra này.");
            }

            return View(resultDetailsViewModel);
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
            // Giải mã `TestId` từ Base64
            var deCodeId = EncodeUrl.Decode(id);

            if (string.IsNullOrEmpty(deCodeId))
            {
                return NotFound();
            }

            string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Sử dụng IQueryable để truy vấn Test
            var testQuery = _context.Tests
                .Include(t => t.Questions)
                    .ThenInclude(q => q.Answers)
                .Where(t => t.TestId == deCodeId)
                .AsQueryable();

            var test = await testQuery.FirstOrDefaultAsync();

            if (test == null)
            {
                return NotFound("Bài kiểm tra không tồn tại hoặc bạn chưa làm bài này.");
            }

            // Truy vấn TestResults bằng IQueryable
            var userTestResultsQuery = _context.TestResults
                .Where(tr => tr.TestId == deCodeId && tr.UserId == userId)
                .Include(tr => tr.User)
                .AsQueryable();

            var userTestResults = await userTestResultsQuery.ToListAsync();

            // Tính toán thời gian làm bài
            var earliestSubmittedResult = await userTestResultsQuery.MinAsync(tr => tr.StartedAt);
            var latestSubmittedResult = await userTestResultsQuery.MaxAsync(tr => tr.SubmittedAt);

            TimeSpan? timeTaken = latestSubmittedResult - earliestSubmittedResult;

            // Thiết lập ViewBag cho view
            ViewBag.TimeTaken = timeTaken;
            ViewBag.LatestSubmittedAt = latestSubmittedResult;
            ViewBag.UserTestResulted = userTestResults;
            ViewBag.UserId = userId;
            ViewBag.TestName = test.TestName;
            ViewBag.TotalScore = userTestResults.Sum(tr => tr.TotalScore);
            ViewBag.ListQuestion = test.Questions.ToList();
            ViewBag.ListTestResult = userTestResults;

            return View(test);
        }




        // TestsController.cs

        [Authorize]
        [Authorize(Roles = "Administrators")]
        public async Task<IActionResult> SelectTest()
        {
            var tests = await _context.Tests.ToListAsync(); // Lấy danh sách các bài test từ database
            return View(tests);
        }

        [Authorize]
        [HttpGet]
        public IActionResult TestDetails(string id, int page = 1)
        {
            // Lưu thời gian bắt đầu vào Session nếu chưa có
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("TestStartTime")))
            {
                HttpContext.Session.SetString("TestStartTime", DateTime.UtcNow.ToString());
            }

            // Lấy thông tin bài test từ database
            var test = _context.Tests
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefault(t => t.TestId == id);

            string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var userMakeTestResult = _context.TestResults.Where(tr => tr.TestId == id && tr.UserId == userId).ToList();
            var questionOfTest = _context.Questions.Where(q => q.TestId == id).ToList();
            if (questionOfTest.Count == userMakeTestResult.Count())
            {

                return NotFound("Bạn đã làm bài này rồi");
           /*     return RedirectToAction("ResultOfTest", new { id = id });*/
            }
            if (test == null)
            {
                return NotFound();
            }
            if (test.IsLocked)
            {
                return NotFound("Bài kiểm tra đã bị khóa"); 
            }
            // Phân trang các câu hỏi
            int PageSize = 1;
            var questions = test.Questions
                .OrderBy(q => q.QuestionId)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var paginatedList = new PaginatedList<Question>(questions, test.Questions.Count, page, PageSize);

            // Truyền các biến cần thiết vào ViewBag để sử dụng trong view
            ViewBag.TestId = id;
            ViewBag.NextPage = page + 1;

            return View(paginatedList);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> MakeTestResult(string testId, string questionId, string selectedAnswer)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Lấy UserId từ Claims
                string userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Lấy thời gian bắt đầu từ Session
                string testStartTimeStr = HttpContext.Session.GetString("TestStartTime");
                DateTime startedAt;
                if (!DateTime.TryParse(testStartTimeStr, out startedAt))
                {
                    // Xử lý nếu thời gian bắt đầu không tồn tại hoặc không hợp lệ
                    return BadRequest("Invalid start time");
                }

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

                // Lấy múi giờ Việt Nam
                TimeZoneInfo vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

                // Lấy thời gian hiện tại theo UTC
                DateTime utcNow = DateTime.UtcNow;

                // Tạo một bản ghi TestResult
                var testResult = new TestResult
                {
                    TestResultId = Guid.NewGuid().ToString(),
                    UserId = userId,
                    TestId = testId,
                    StartedAt = TimeZoneInfo.ConvertTimeFromUtc(startedAt, vietnamTimeZone),
                    SubmittedAt = TimeZoneInfo.ConvertTimeFromUtc(utcNow, vietnamTimeZone),
                    IsCorrect = isCorrect,
                    QuestionId = questionId,
                    SelectedAnswer = selectedAnswer,
                    TotalScore = isCorrect ? 10 : 0 // Giả sử điểm số mặc định là 10 nếu đúng
                };

                // Lưu TestResult vào database
                _context.Add(testResult);
                await _context.SaveChangesAsync();

                // Xóa thời gian bắt đầu khỏi session sau khi hoàn thành bài kiểm tra
                HttpContext.Session.Remove("TestStartTime");

                return RedirectToAction(nameof(Index)); // Hoặc trả về một view khác tùy vào yêu cầu của bạn
            }
            else
            {
                // Nếu chưa đăng nhập thì chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Account");
            }
        }



        // GET: Tests
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            var tests = from t in _context.Tests
                        select t;

            // Apply search filter if searchString is not null or empty
            if (!string.IsNullOrEmpty(searchString))
            {
                tests = tests.Where(t => t.TestName.Contains(searchString));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy userId của người dùng hiện tại

            // Lấy danh sách các TestResult của người dùng hiện tại
            var userTestResults = await _context.TestResults
                .Where(tr => tr.UserId == userId)
                .Select(tr => tr.TestId)
                .ToListAsync();
            ViewBag.UserTestResulted = userTestResults; // Lưu danh sách TestId mà người dùng đã làm vào ViewBag

            var lisquestion = await _context.Questions.Select(tr => tr.TestId).ToListAsync();
            ViewBag.ListQuestion = lisquestion;

            var allUserTestResults = await _context.TestResults
                .GroupBy(tr => tr.TestId)
                .Select(g => new { TestId = g.Key, UserCount = g.Select(tr => tr.UserId).Distinct().Count() })
                .ToListAsync();
            ViewBag.countUserMakeTest = allUserTestResults;

            // Kiểm tra quyền Admin
            var isAdmin = User.IsInRole("Administrators");

            if (!isAdmin)
            {
                // Lọc các bài test bị khóa và những bài test không có câu hỏi
                tests = tests.Where(t => !t.IsLocked && _context.Questions.Any(q => q.TestId == t.TestId));
            }

            // Define page size
            int pageSize = 5;

            // Create paginated list
            var paginatedTests = await PaginatedList<Test>.CreateAsync(tests.AsNoTracking(), pageNumber ?? 1, pageSize);

            return View(paginatedTests);
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
