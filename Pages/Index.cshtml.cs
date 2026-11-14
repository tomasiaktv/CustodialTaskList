
using CustodialTaskList.Data;
using CustodialTaskList.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace CustodialTaskList.Pages;

public class IndexModel : PageModel
{
    private readonly DailyTaskService _taskService;
    private readonly TaskDbContext _db;

    public IndexModel(
        DailyTaskService taskService,
        TaskDbContext db)
    {
        _taskService = taskService;
        _db = db;
    }

    public void OnGet()
    {
    }

    // GET /?handler=Tasks&shift=Morning
    public async Task<IActionResult> OnGetTasksAsync(
        string shift = "Morning")
    {
        if (shift != "Morning" && shift != "Evening")
            return BadRequest("Invalid shift.");

        var tasks = await _taskService.GetTodayAsync(shift);

        return new JsonResult(tasks.Select(t => new
        {
            id = t.Id,
            name = t.TaskName,
            room = t.Room,
            completed = t.IsCompleted,
            initials = t.CompletedByInitials,
            completedAt = t.CompletedAt?
                .ToLocalTime().ToString("h:mm tt")
        }));
    }

    // POST /?handler=Complete
    public async Task<IActionResult> OnPostCompleteAsync(
        int taskId,
        string initials)
    {
        initials = (initials ?? "").Trim().ToUpperInvariant();

        if (!Regex.IsMatch(initials, "^[A-Z]{1,4}$"))
            return BadRequest("Enter 1 to 4 letters.");

        var task = await _db.DailyTasks
            .FirstOrDefaultAsync(t =>
                t.Id == taskId &&
                t.ScheduledDate ==
                    DateOnly.FromDateTime(DateTime.Today));

        if (task == null)
            return NotFound();

        if (task.IsCompleted)
            return new ConflictObjectResult("Task already completed.");

        task.IsCompleted = true;
        task.CompletedByInitials = initials;
        task.CompletedAt = DateTimeOffset.Now;

        await _db.SaveChangesAsync();

        return new JsonResult(new
        {
            success = true,
            initials = task.CompletedByInitials,
            completedAt = task.CompletedAt?
                .ToString("h:mm tt")
        });
    }

    // POST /?handler=Undo
    public async Task<IActionResult> OnPostUndoAsync(int taskId)
    {
        // Find the task in today's task list.
        var task = await _db.DailyTasks
            .FirstOrDefaultAsync(t =>
                t.Id == taskId &&
                t.ScheduledDate ==
                    DateOnly.FromDateTime(DateTime.Today));

        // Make sure the task exists.
        if (task == null)
        {
            return NotFound();
        }

        // Only completed tasks can be undone.
        if (!task.IsCompleted)
        {
            return new ConflictObjectResult(
                "This task is already incomplete.");
        }

        // Reset the task's completion status.
        task.IsCompleted = false;

        // Remove the employee's initials.
        task.CompletedByInitials = null;

        // Remove the completion time.
        task.CompletedAt = null;

        // Save the changes to SQLite.
        await _db.SaveChangesAsync();

        // Tell the webpage the undo succeeded.
        return new JsonResult(new
        {
            success = true,
            message = "Task completion undone."
        });
    }

}