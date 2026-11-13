# CustodialTaskList

**A touchscreen-friendly custodial task management system built with ASP.NET Core and SQLite.**

CustodialTaskList replaces paper-based cleaning schedules with a shared daily task board. Custodians can view their responsibilities, record completed work using their initials, and undo accidental completions. Supervisors manage one-time and recurring schedules through a password-protected dashboard.

The application is **deployed and operational** on a repurposed Windows 10 Pro touchscreen kiosk. It is hosted using IIS and is accessible through HTTPS to authorized devices on the organization's internal network. The kiosk and mobile browsers access the same application and SQLite database.

> **Deployment note:** The workplace deployment and its internal DNS, certificates, network rules, credentials, and live database are not included in this repository. Windows 10 Pro reached its standard end of support in October 2025; organizations should plan a supported operating system or an IT-approved security-update arrangement for network-connected deployments.

## Features

### Custodian task board

- Shows tasks scheduled for the current day, separated into **Morning** and **Evening** shifts.
- Selects the current shift automatically and allows manual switching.
- Updates task information automatically (approximately every 10 seconds), so changes made from other devices appear without a full-page refresh.
- Records task completion with employee initials and a completion timestamp.
- Provides an **Undo Completion** action for tasks marked complete by mistake.
- Displays completion status and progress for the selected shift.
- Includes a touchscreen keyboard for entering initials.

### Recurring schedules

Supervisors can create tasks with a start date, shift, and selected weekday(s):

| Schedule | Behavior |
| --- | --- |
| One-time | Runs on the chosen start date. |
| Weekly | Runs every selected weekday beginning on or after the start date. |
| Monthly | Runs on the **first, second, third, fourth, or last** occurrence of a selected weekday each month. |
| Quarterly | Uses the selected weekday occurrence every three months, counted from the start month. |
| Semiannually | Uses the selected weekday occurrence every six months, counted from the start month. |
| Annually | Uses the selected weekday occurrence every twelve months, counted from the start month. |

For example, a monthly task configured for **First + Monday** runs on the first Monday of each month—not on every Monday or the same numerical date each month. Multiple weekdays can be selected when creating a repeating task; each selected weekday is stored as a task template.

### Supervisor dashboard

- Password-protected Admin login.
- **Morning / Evening** filter for the active tasks on the selected day.
- Compact, read-only task cards showing the task name, shift, scheduled day, repeating status, and frequency.
- **Edit Task** popup for updating task names, shifts, start dates, weekdays, recurrence, and weekday occurrence.
- Create new tasks, deactivate individual templates, or remove all active task templates.
- Review historical daily task records, including completed and incomplete tasks, initials, and completion times.
- Touchscreen-friendly on-screen keyboard for entering the supervisor password and task names.

Deactivating a task template stops future scheduling without intentionally deleting previous daily task history.

### Kiosk and mobile access

- Microsoft Edge runs in full-screen **Assigned Access** kiosk mode and opens the internal HTTPS site automatically.
- The application is hosted by **Internet Information Services (IIS)** on the kiosk PC.
- Internal DNS provides a consistent hostname; an HTTPS certificate is configured and trusted on the devices that need access.
- Custodians on an authorized internal network can open the application in a phone browser and complete tasks using the same shared task list.
- Supervisors can access the Admin interface from an authorized laptop or the kiosk.
- Windows Firewall inbound access is scoped to the organization's approved internal network and the application service port, as coordinated with IT.
- The live SQLite database and supervisor settings are stored separately from the published application files to preserve them across updates.

**Network access is internal only.** The repository does not include the organization's actual hostname, IP addressing, firewall rules, or certificate/private-key material. This deployment is not intended to be exposed directly to the public internet.

## Technology stack

| Technology | Role |
| --- | --- |
| C# / ASP.NET Core (.NET 10) | Application, request handling, and scheduling logic |
| Razor Pages | Server-rendered pages and Admin forms |
| JavaScript | Shift selection, polling, task completion, and virtual keyboards |
| HTML, CSS, Bootstrap | Responsive kiosk and mobile interface |
| Entity Framework Core / SQLite | Task templates, daily records, and migrations |
| IIS / ASP.NET Core Hosting Bundle | Windows application hosting |
| Microsoft Edge Assigned Access | Full-screen touchscreen kiosk |
| Visual Studio, Git, GitHub | Development and version control |

## Getting started (development)

### Prerequisites

- .NET 10 SDK.
- Visual Studio with **ASP.NET and web development** support (or a compatible .NET editor).
- A modern browser.

### Run locally

1. Clone the repository and open the solution in Visual Studio.
2. Select **CustodialTaskList** as the startup project.
3. Configure a development supervisor password using the instructions below.
4. Ensure your local SQLite connection string points to a writable development database, and apply the included EF Core migrations as needed.
5. Run the project using its Development launch profile and open the local address shown by Visual Studio.

The production database, real custodial task records, and production secrets are **not** supplied. A new development database will not automatically contain the workplace's tasks.

### Configure the development Admin password

There is **no default Admin password included in this repository**. To choose your own password for local development:

1. In Visual Studio, right-click the **CustodialTaskList** project (not the solution).
2. Select **Manage User Secrets**.
3. Add the following to `secrets.json`, replacing the placeholder with your own password:

   ```json
   {
     "Supervisor": {
       "Password": "ChooseYourOwnDevelopmentPassword"
     }
   }
   ```

4. Save the file, start the application in **Development** mode, and use your chosen password on the Admin login page.

ASP.NET Core User Secrets live outside the project directory, but **are not encrypted**. Use them for local development only, never commit them, and do not reuse a real production password.

### Production supervisor password

Production authentication uses the `Supervisor:PasswordHash` configuration setting, verified with ASP.NET Core's `PasswordHasher`. The solution includes a separate **SupervisorPasswordGenerator** console project for generating a compatible hash without entering the password in source code.

The production installation reads its supervisor settings from a protected file outside the published application directory. Keep the password hash and configuration file restricted to authorized administrators and the application identity. Do not put credentials or certificates with private keys in this repository or under `wwwroot`.

## Architecture

```text
                  Authorized internal network
                  (HTTPS / internal DNS)
                             |
             +---------------+---------------+
             |                               |
       Kiosk Edge browser              Phone / laptop
             |                               |
             +---------------+---------------+
                             |
                      IIS / ASP.NET Core
                             |
                    Razor Pages + services
                             |
                        EF Core / SQLite
                             |
                     Task and history data
```

The host runs one central application and database. Phone and laptop access does not require additional copies of SQLite on client devices.

## Deployment overview

The deployed system uses a **folder publish** of the ASP.NET Core application, transferred to the Windows kiosk and hosted through IIS with the appropriate ASP.NET Core Hosting Bundle.

A typical update workflow is:

1. Back up the live SQLite database and retain a rollback copy of the installed application.
2. Make code changes locally, build, and publish a **Release** version.
3. Commit and push source changes to GitHub.
4. Stop the dedicated IIS application pool during the maintenance window.
5. Replace the published application files **without overwriting the production database or supervisor configuration**.
6. Apply required Entity Framework Core migrations carefully and restart the application pool.
7. Verify Admin login, the daily task board, scheduling, task history, and mobile access.

Database files should not be edited while the running application is actively writing to them. Back up the database before schema changes or manual SQLite maintenance.

## Security and configuration

This public repository does **not** include:

- The live SQLite database or employee completion history.
- Supervisor passwords, production password hashes, or User Secrets.
- HTTPS certificate private keys or `.pfx`/`.p12` credentials.
- Internal DNS, static IP, or organization-specific firewall configuration.

The actual deployment is restricted to the authorized internal network. HTTPS certificate trust and firewall scope are managed in coordination with the organization's IT team. Production installations should use supported operating systems and appropriate backup, access-control, and patching practices.

## Project status

**Deployed and operational.** The core kiosk workflow, internal phone access, supervisor management, weekday-occurrence scheduling, task history, and touchscreen editing are implemented. Future improvements, if needed, could include richer reports, history search/filtering, and more detailed task audit records.

## About

CustodialTaskList is an independent software development project created to solve a practical workplace scheduling problem while developing hands-on experience in full-stack .NET development, database design, responsive UI, authentication, Windows/IIS deployment, and internal-network hosting.

**Author:** Ian Tomasiak — Computer Science Student
