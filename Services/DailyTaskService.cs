
using CustodialTaskList.Data;
using CustodialTaskList.Models;
using Microsoft.EntityFrameworkCore;

namespace CustodialTaskList.Services;

public class DailyTaskService
{
    private readonly TaskDbContext _db;

    public DailyTaskService(TaskDbContext db)
    {
        _db = db;
    }

    // Determines whether a scheduled task
    // should appear on a specific date.

    private static bool ShouldRun(
        TaskTemplate task,
        DateOnly date)
    {
        // Inactive tasks should never appear.
        if (!task.IsActive)
            return false;

        // Check that today matches the task's weekday.
        if (task.DayOfWeek != (int)date.DayOfWeek)
            return false;

        // Compatibility with older task templates.
        if (task.StartDate == default)
            return task.IsRepeating &&
                   task.RepeatFrequency == "Weekly";

        // Never run before the starting date.
        if (date < task.StartDate)
            return false;

        // One-time tasks only run on their start date.
        if (!task.IsRepeating)
            return date == task.StartDate;

        // Weekly tasks remain unchanged.
        if (task.RepeatFrequency == "Weekly")
            return true;

        // Calculate how many months have passed
        // since the task's starting month.
        int monthsPassed =
            (date.Year - task.StartDate.Year) * 12
            + date.Month - task.StartDate.Month;

        // Determine the frequency interval.
        int interval = task.RepeatFrequency switch
        {
            "Monthly" => 1,
            "Quarterly" => 3,
            "SemiAnnually" => 6,
            "Semi-Annually" => 6,
            "Annually" => 12,
            _ => 0
        };

        // Reject unsupported frequencies.
        if (interval == 0)
            return false;

        // Check whether this is a scheduled month.
        if (monthsPassed < 0 ||
            monthsPassed % interval != 0)
            return false;

        // Check which occurrence of the weekday
        // is scheduled for the current month.
        return MatchesWeekOccurrence(
            date,
            task.WeekOccurrence);
    }

    private static bool MatchesWeekOccurrence(
        DateOnly date,
        string? occurrence)
    {
        // Determine whether this is the first,
        // second, third, or fourth occurrence.
        int weekNumber = ((date.Day - 1) / 7) + 1;

        // Is there another occurrence of this
        // weekday seven days later?
        bool isLast =
            date.AddDays(7).Month != date.Month;

        return occurrence switch
        {
            "First" => weekNumber == 1,
            "Second" => weekNumber == 2,
            "Third" => weekNumber == 3,
            "Fourth" => weekNumber == 4,
            "Last" => isLast,
            _ => false
        };
    }

    public async Task<List<DailyTask>> GetTodayAsync(
        string shift)
    {
        if (shift != "Morning" && shift != "Evening")
            throw new ArgumentException("Invalid shift.");

        var today = DateOnly.FromDateTime(
            DateTime.Today);

        // Retrieve active task templates
        // for today's weekday and selected shift.
        var templates = await _db.TaskTemplates
            .AsNoTracking()
            .Where(t =>
                t.IsActive &&
                t.DayOfWeek == (int)today.DayOfWeek &&
                t.Shift == shift)
            .ToListAsync();

        // Apply the repeating schedule rules.
        var scheduledTemplates = templates
            .Where(t => ShouldRun(t, today))
            .ToList();

        var scheduledIds = scheduledTemplates
            .Select(t => t.Id)
            .ToHashSet();

        // Find tasks already generated today.
        var existingIds = await _db.DailyTasks
            .Where(t => t.ScheduledDate == today)
            .Select(t => t.TaskTemplateId)
            .ToListAsync();

        var existingSet = existingIds.ToHashSet();

        // Generate missing daily tasks.
        foreach (var template in scheduledTemplates)
        {
            if (existingSet.Contains(template.Id))
                continue;

            _db.DailyTasks.Add(new DailyTask
            {
                TaskTemplateId = template.Id,
                TaskName = template.TaskName,
                Shift = template.Shift,
                ScheduledDate = today,
                IsCompleted = false
            });
        }

        await _db.SaveChangesAsync();

        // Return only tasks scheduled for today.
        var dailyTasks = await _db.DailyTasks
            .AsNoTracking()
            .Where(t =>
                t.ScheduledDate == today &&
                t.Shift == shift)
            .OrderBy(t => t.TaskName)
            .ToListAsync();

        return dailyTasks
            .Where(t => scheduledIds.Contains(t.TaskTemplateId))
            .ToList();
    }
}
