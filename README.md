# CustodialTaskList



### A Touchscreen-Friendly Custodial Task Management System



CustodialTaskList is a web-based task management application developed using \*\*ASP.NET Core, C#, Razor Pages, JavaScript, and SQLite**.



The application was designed to replace traditional paper-based cleaning schedules with an interactive digital system. It allows custodial staff to view daily responsibilities, mark tasks as completed, and track progress throughout their shifts.



Originally developed as a practical solution for a workplace environment, the application has been deployed on a repurposed Windows touchscreen computer running Microsoft Edge in kiosk mode.



---



## Project Overview



CustodialTaskList provides a centralized platform for managing recurring and one-time custodial responsibilities.



The application focuses on simplicity, accessibility, and usability, allowing employees to interact with the system using a touchscreen without requiring individual user accounts.



Supervisors can manage scheduled tasks through a separate password-protected administration interface.



### Project Goals



- Replace paper-based custodial task schedules.

- Provide a clear, interactive daily task list.

- Support morning and evening work shifts.

- Allow employees to record task completion using their initials.

- Maintain completion records in a persistent database.

- Reduce repetitive administrative work through automated scheduling.

- Repurpose existing hardware instead of purchasing a dedicated kiosk system.



---



## Features



### Daily Task Management



- Displays tasks scheduled for the current day.

- Separates responsibilities into Morning and Evening shifts.

- Automatically selects the appropriate shift based on local time.

- Allows employees to switch between shifts manually.

- Displays task completion status and employee initials.

- Provides an option to undo accidental task completions.

- Shows a completion counter for each shift.



### Automated Scheduling



Supervisors can configure tasks using several scheduling options:



- One-time tasks

- Weekly tasks

- Monthly tasks

- Quarterly tasks

- Semiannual tasks

- Annual tasks



Weekly tasks can be assigned to specific weekdays, allowing recurring schedules to be managed without manually recreating tasks.



### Supervisor Administration



A password-protected administration interface allows authorized supervisors to:



- Create and manage custodial tasks.

- Assign tasks to specific shifts.

- Configure recurring schedules.

- Review task completion history.

- Deactivate active tasks.

- Manage the daily task schedule.



Supervisor credentials are protected using password hashing rather than plaintext password storage.

### Setting Up the Development Admin Password

CustodialTaskList requires a supervisor password to access
the administration dashboard.

The repository does not include a default password.

To configure your own password for local development:

1. Open the solution in Visual Studio.
2. Right-click the CustodialTaskList project.
3. Select "Manage User Secrets".
4. Add the following configuration:

```json
{
  "Supervisor": {
    "Password": "YourChosenPassword"
  }
}
```

5. Save the file.
6. Run the application in Development mode.
7. Select Admin and log in with your chosen password.

User Secrets are stored outside the project directory
and should not be committed to GitHub.

For production deployments, configure
Supervisor:PasswordHash using ASP.NET Core PasswordHasher.
Do not use plaintext passwords in production.



### Touchscreen Support



The interface was designed for interaction with a touchscreen kiosk.



Features include:



- Large, accessible buttons.

- Responsive layouts.

- Touchscreen-friendly task completion controls.

- On-screen keyboard for entering employee initials.

- Draggable virtual keyboard for administrative input.

- Simplified navigation.



### Automatic Updates



The application retrieves updated task information from the server every 10 seconds.



This allows task completion and administrative changes to appear without manually refreshing the webpage.



The application also automatically switches between scheduled shifts:



| Time | Displayed Shift |

|---|---|

| 6:00 AM – 1:59 PM | Morning |

| 2:00 PM – 9:59 PM | Evening |

| 10:00 PM – 5:59 AM | Morning (Default) |



### Kiosk Deployment



The application has been deployed on a repurposed Windows 11 touchscreen computer.



The deployment uses:



- Internet Information Services (IIS)

- ASP.NET Core Hosting Bundle

- Microsoft Edge kiosk mode

- Windows Assigned Access

- Automatic kiosk account sign-in

- Windows Bubbles screen saver

- Persistent SQLite database storage



The kiosk launches the application automatically after startup, providing a dedicated task management interface.



---



## Technologies Used



| Technology | Purpose |

|---|---|

| C# | Server-side application logic |

| ASP.NET Core (.NET 10) | Web application framework |

| Razor Pages | Server-rendered web interface |

| JavaScript | Client-side interactions and automatic updates |

| HTML5 | Application structure |

| CSS3 | Custom styling and layout |

| Bootstrap | Responsive interface components |

| SQLite | Persistent task and completion data |

| Microsoft IIS | Windows web hosting |

| Microsoft Edge | Touchscreen kiosk browser |

| Visual Studio | Development environment |

| Git / GitHub | Source control and project documentation |



---



## Application Workflow



### Employee Workflow



1. Open the application on the touchscreen kiosk.

2. View the currently scheduled shift and daily responsibilities.

3. Select \*\*Complete\*\* for a finished task.

4. Enter employee initials using the on-screen keyboard.

5. Submit the completion.

6. View the updated completion status.



If a task is completed accidentally, the employee can use the Undo Completion option.



### Supervisor Workflow



1. Navigate to the administration page.

2. Authenticate using the supervisor password.

3. Create a new task.

4. Assign the task to a shift.

5. Configure its start date and recurrence schedule.

6. Save the task.

7. Review the task list and completion history.



---



## Application Architecture



CustodialTaskList uses a server-side web architecture with client-side JavaScript for interactive features.



**Frontend:** Razor Pages, HTML, CSS, Bootstrap, and JavaScript provide the user interface.



**Backend:** ASP.NET Core handles application requests, scheduling logic, task management, and database operations.



**Database:** SQLite stores task definitions and completion information.



**Hosting:** IIS hosts the application on a Windows computer, allowing the kiosk browser to communicate with the locally hosted application.



**Client Updates:** JavaScript periodically requests updated task information, allowing the interface to reflect database changes without requiring a full-page refresh.



---



## Development and Deployment



The application was developed in Visual Studio using ASP.NET Core and .NET 10.



During development, it can be launched through Visual Studio using the configured development profile.



For deployment, the application is published and hosted using IIS with the appropriate ASP.NET Core runtime.



The production environment stores operational data and supervisor settings separately from the published application files.



### Requirements



- .NET 10 SDK for development

- Visual Studio with ASP.NET and web development tools, or another compatible .NET development environment

- SQLite database support

- A modern web browser

- IIS and the ASP.NET Core Hosting Bundle for the Windows IIS deployment scenario



### Security and Configuration



This repository is intended to contain application source code only.



The following production resources are not included:



- Operational SQLite databases

- Employee task-completion records

- Supervisor password configuration

- Authentication keys

- Production-specific network settings

- Environment-specific secrets



Deployment environments must supply their own configuration and initialize their own application data.



---



## Design Considerations



Several design decisions were made specifically for a workplace touchscreen environment.



**Accessibility:** Large controls and clear labels simplify interaction for employees with varying levels of technical experience.



**Reliability:** Persistent database storage and automatic browser startup help maintain consistent availability.



**Maintainability:** Recurring task schedules reduce the need for supervisors to manually update daily responsibilities.



**Hardware Reuse:** The application provides a practical use for an existing touchscreen computer that would otherwise remain unused.



**Security:** A separate administration interface and password hashing protect supervisory functions.



**Usability:** On-screen keyboards allow the application to operate without a physical keyboard during normal use.



---



## Future Improvements



Potential future enhancements include:



- Secure HTTPS access using an internal network hostname.

- Access from authorized devices on the organization's local network.

- Additional administrative reporting.

- Task completion summaries and statistics.

- Improved filtering and searching of historical task records.

- Additional scheduling and notification capabilities.



---



## Project Status



**Status: Deployed and Operational**



The application is operational on a Windows 11 touchscreen kiosk.



Additional network configuration and enhancements are planned.



---



## About This Project



CustodialTaskList was developed as an independent software development project focused on solving a practical workplace problem.



The project demonstrates experience with:



- Full-stack web application development

- C# and ASP.NET Core

- Relational database integration

- JavaScript and interactive user interfaces

- Authentication and application security

- Recurring scheduling logic

- Windows application deployment

- IIS configuration

- Touchscreen kiosk implementation

- Practical software troubleshooting



The goal was to create a useful, maintainable application while gaining hands-on experience with the complete software development lifecycle, from initial design through deployment.



---



## Author



**Ian Tomasiak**



Computer Science Student



Developed as a practical application of software engineering, web development, and system deployment concepts.

