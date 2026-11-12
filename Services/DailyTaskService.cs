
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
        // Never display inactive tasks.
        if (!task.IsActive)
            return false;

        // Must match the selected weekday.
        if (task.DayOfWeek != (int)date.DayOfWeek)
            return false;

        // Support older tasks created before
        // we added repeating schedules.
        if (task.StartDate == default)
            return true;

        // Do not display tasks before their start date.
        if (date < task.StartDate)
            return false;

        // Non-repeating tasks run only once.
        if (!task.IsRepeating)
            return date == task.StartDate;

        // Weekly tasks run every selected weekday.
        if (task.RepeatFrequency == "Weekly")
            return true;

        // Calculate the number of months
        // since this task was created.
        int monthsPassed =
            (date.Year - task.StartDate.Year) * 12
            + date.Month - task.StartDate.Month;

        switch (task.RepeatFrequency)
        {
            case "Monthly":
                return true;

            case "Quarterly":
                return monthsPassed % 3 == 0;

            case "SemiAnnually":
                return monthsPassed % 6 == 0;

            case "Annually":
                return monthsPassed % 12 == 0;

            default:
                return false;
        }
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
