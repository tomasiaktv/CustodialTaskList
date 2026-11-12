
using CustodialTaskList.Data;
using CustodialTaskList.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CustodialTaskList.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly TaskDbContext _db;

    public IndexModel(TaskDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int Day { get; set; } = 1;

    public List<TaskTemplate> Tasks { get; set; } = new();

    public List<DailyTask> TaskHistory { get; set; } = new();


    [TempData]
    public string? Message { get; set; }

    public string DayName =>
        Enum.GetName(typeof(DayOfWeek), Day) ?? "Monday";


    public async Task OnGetAsync()
    {
        if (Day < 1 || Day > 5)
            Day = 1;

        // Load active scheduled tasks.
        await LoadTasksAsync();

        // Load historical daily task records.
        await LoadTaskHistoryAsync();
    }



    private async Task LoadTaskHistoryAsync()
    {
        // Get today's date.
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Retrieve up to 100 tasks from previous days.
        TaskHistory = await _db.DailyTasks
            .AsNoTracking()
            .Where(t => t.ScheduledDate < today)
            .OrderByDescending(t => t.ScheduledDate)
            .ThenBy(t => t.Shift)
            .ThenBy(t => t.TaskName)
            .Take(100)
            .ToListAsync();
    }


    private async Task LoadTasksAsync()
    {
        Tasks = await _db.TaskTemplates
            .AsNoTracking()
            .Where(t => t.DayOfWeek == Day && t.IsActive)
            .OrderBy(t => t.Shift)
            .ThenBy(t => t.TaskName)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync(
        int id,
        string taskName,
        string shift,
        int day,
        List<int>? selectedDays,
        bool isRepeating,
        string? repeatFrequency,
        DateOnly startDate)

    {
        // Validate the currently selected dashboard day.
        if (day < 1 || day > 5)
            return BadRequest();

        taskName = taskName?.Trim() ?? "";

        // Make sure the task name and shift are valid.
        if (string.IsNullOrWhiteSpace(taskName) ||
            taskName.Length > 150 ||
            (shift != "Morning" && shift != "Evening"))
        {
            Message = "Please enter a valid task and shift.";
            return RedirectToPage(new { day });
        }

        if (id == 0)
        {

            var days = selectedDays?
                .Distinct()
                .ToList() ?? new List<int>();

            // For a one-time task, automatically use
            // the weekday of the selected start date.
            if (!isRepeating)
            {
                days = new List<int>
    {
        (int)startDate.DayOfWeek
    };
            }

            // Make sure the selected date is valid.
            if (startDate == default)
            {
                Message = "Please select a valid start date.";
                return RedirectToPage(new { day });
            }

            // Only allow Monday through Friday.
            if (days.Count == 0 ||
                days.Any(d => d < 1 || d > 5))
            {
                Message = "Please select a weekday (Monday-Friday).";
                return RedirectToPage(new { day });
            }

            // Require at least one valid weekday.
            if (days.Count == 0 ||
                days.Any(d => d < 0 || d > 6))
            {
                Message = "Please select at least one weekday.";
                return RedirectToPage(new { day });
            }


            // Make sure a valid start date was provided.
            if (startDate == default)
            {
                Message = "Please select a valid start date.";
                return RedirectToPage(new { day });
            }

            // Check the repeat frequency.
            string frequency = "None";

            if (isRepeating)
            {
                string[] allowedFrequencies =
                {
                "Weekly",
                "Bi-Weekly",
                "Monthly",
                "Quarterly",
                "SemiAnnually",
                "Annually"
            };

                if (repeatFrequency == null ||
                    !allowedFrequencies.Contains(repeatFrequency))
                {
                    Message = "Please select a valid repeat frequency.";
                    return RedirectToPage(new { day });
                }

                frequency = repeatFrequency;
            }

            // Create a task template for each selected weekday.
            foreach (int selectedDay in days)
            {

                var newTask = new TaskTemplate
                {
                    TaskName = taskName,
                    Shift = shift,
                    DayOfWeek = selectedDay,
                    IsRepeating = isRepeating,
                    RepeatFrequency = frequency,
                    StartDate = startDate,
                    IsActive = true
                };

                _db.TaskTemplates.Add(newTask);
            }
        }
        else
        {
            // Update an existing task.
            var task = await _db.TaskTemplates.FindAsync(id);

            if (task == null ||
                !task.IsActive ||
                task.DayOfWeek != day)
            {
                return NotFound();
            }

            task.TaskName = taskName;
            task.Shift = shift;

            // Existing schedule settings remain unchanged.

            // Keep unfinished tasks for today synchronized.
            var today = DateOnly.FromDateTime(DateTime.Today);

            var todayTask = await _db.DailyTasks
                .FirstOrDefaultAsync(t =>
                    t.TaskTemplateId == id &&
                    t.ScheduledDate == today &&
                    !t.IsCompleted);

            if (todayTask != null)
            {
                todayTask.TaskName = taskName;
                todayTask.Shift = shift;
            }
        }

        await _db.SaveChangesAsync();

        Message = "Task schedule saved successfully.";

        return RedirectToPage(new { day });
    }


    public async Task<IActionResult> OnPostDeleteAsync(
        int id,
        int day)
    {
        if (day < 1 || day > 5)
            return BadRequest();

        var task = await _db.TaskTemplates.FindAsync(id);

        if (task == null)
            return NotFound();

        // Deactivate rather than remove historical identity.
        task.IsActive = false;

        await _db.SaveChangesAsync();

        Message = "Task removed from recurring schedule.";

        return RedirectToPage(new { day });
    }


    public async Task<IActionResult> OnPostRemoveAllAsync()
    {
        // Find all currently active task templates.
        var activeTasks = await _db.TaskTemplates
            .Where(t => t.IsActive)
            .ToListAsync();

        // Deactivate each task.
        foreach (var task in activeTasks)
        {
            task.IsActive = false;
        }

        // Save the changes to the database.
        await _db.SaveChangesAsync();

        // Display a confirmation message.
        Message = $"{activeTasks.Count} active tasks removed.";

        // Return to the Admin dashboard.
        return RedirectToPage(new { day = Day });
    }


    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync();
        return RedirectToPage("/Admin/Login");
    }
}