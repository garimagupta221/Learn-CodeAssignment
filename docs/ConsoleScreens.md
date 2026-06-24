# PRM Tool – Console App Screens

All screens share the same header (printed by `ConsoleHelper.PrintHeader`):

```
Role: <Role>
Date: <Day>, <Month> <D>, <Year>

```

---

## Navigation Flow

```
StartScreen
└── LoginScreen
    ├── ChangePasswordScreen   (forced when ForcePasswordChange = true)
    ├── AdminMenu
    ├── ManagerMenu
    └── EmployeeMenu
```

> **V4 change:** `SignUpScreen` has been removed. Public self-registration is disabled. All accounts
> (Admin, Manager, Employee) are created exclusively by an Admin via `ManageUsersScreen → Add New User`.

---

## Common Screens

### StartScreen
`PrmClient/UI/Common/StartScreen.cs`

```
Role: Guest
Date: Sunday, June 1, 2026

Welcome to PRM Tool

  1. Login
  2. Exit

Enter option: _
```

> **V4 change:** "Sign Up" option removed. Menu is now 2 options (1 Login, 2 Exit).

---

### ~~SignUpScreen~~ *(Removed in V4)*
`PrmClient/UI/Common/SignUpScreen.cs` — **file deleted.**

Public self-registration is no longer permitted. All accounts (Admin, Manager, Employee)
are created exclusively by an Admin via `ManageUsersScreen → Add New User`.
`POST /api/auth/signup` is now protected by `[Authorize(Roles = "Admin")]`.

---

### LoginScreen
`PrmClient/UI/Common/LoginScreen.cs`

```
Role: Guest
Date: Sunday, June 1, 2026

Login

Username: _
Password: _
```

---

### SignUpScreen
`PrmClient/UI/Common/SignUpScreen.cs`

```
Role: Guest
Date: Sunday, June 1, 2026

Sign Up

Full Name: _
Email: _
Username: _
Password: _
Role (1: Manager, 2: Employee): _
```

---

### ChangePasswordScreen
`PrmClient/UI/Common/ChangePasswordScreen.cs`

```
Role: <Role>
Date: Sunday, June 1, 2026

Change Password

New Password: _
```

---

## Admin Screens

### AdminMenu
`PrmClient/UI/Admin/AdminMenu.cs`

```
Role: Admin
Date: Sunday, June 1, 2026

Admin Panel

  1. Manage Employees
  2. Manage Projects
  3. View Allocations
  4. Manage Users
  5. System Config
  6. Logout

Enter option: _
```

---

### ManageEmployeesScreen
`PrmClient/UI/Admin/ManageEmployeesScreen.cs`

```
Role: Admin
Date: Sunday, June 1, 2026

  1. View All Employees
  2. Update Employee
  3. Deactivate Employee
  4. Manage Employee Skills
  5. Assign Manager
  6. Back

Enter option: _
```

> **V4 changes:**
> - "Add Employee Profile" removed from this menu (handled elsewhere).
> - Skill options (Assign / Update / Remove) consolidated under option 4 "Manage Employee Skills" sub-menu.
> - New option 5 "Assign Manager" added (Screen 3.1.4).

**Sub-flow – Manage Employee Skills (option 4):**
```
  1. Assign Skill to Employee
  2. Update Employee Skill
  3. Remove Employee Skill

  Enter option: _
```

**Sub-flow – Assign Manager (option 5 — Screen 3.1.4):**
```
  Employee User ID : _
  Manager User ID  : _

  Manager assigned successfully.
```

---

### ManageProjectsScreen
`PrmClient/UI/Admin/ManageProjectsScreen.cs`

```
Role: Admin
Date: Sunday, June 1, 2026

  1. Create Project
  2. View All Projects
  3. Update Project Details
  4. Manage Milestones
  5. Back

Enter option: _
```

> **V4 changes:** Option 3 renamed to "Update Project Details". All sub-flows updated with Story Points.

**Sub-flow – Create Project (option 1 — Screen 3.2.1):**
```
  Project Name        : _
  Description         : _
  Start Date          : (DD-MM-YYYY) _
  End Date            : (DD-MM-YYYY) _
  Status              : (1) PLANNED   (2) ACTIVE   (3) ON_HOLD
  Assign Manager      : (Enter Manager ID) _
  Total Story Points  : _

  ──────────────────────────────────────────────
  [S] Save     [B] Back
```

**Sub-flow – View All Projects (option 2 — Screen 3.2.2):**
```
  ID    Name              Manager        End Date     Status     SP Done/Total
  ──────────────────────────────────────────────────────────────────────────────
  201   Alpha Portal       1              30-Jun-26    ACTIVE     40 / 120
  ──────────────────────────────────────────────────────────────────────────────
```

**Sub-flow – Update Project Details (option 3 — Screen 3.2.3):**
```
  Enter Project ID: _

  ── Alpha Portal ───────────────────────────────
  Project Name         : Alpha Portal          (editable)
  Description          : Customer web portal   (editable)
  Start Date           : 01-Jan-26             (editable)
  End Date             : 30-Jun-26             (editable)
  Status               : (1) PLANNED   (2) ACTIVE   (3) ON_HOLD   (4) COMPLETED
  Assign Manager       : (Enter Manager ID)    (editable)
  Total Story Points   : 120                   (editable)
  ──────────────────────────────────────────────
  [S] Save     [B] Back
```

---

### ManageMilestonesScreen
`PrmClient/UI/Admin/ManageMilestonesScreen.cs`

```
Role: Admin
Date: Sunday, June 1, 2026

  Enter Project ID: 201

  ── Alpha Portal ───────────────────────────────
  #    Title               Due Date     Story Pts   Status
  ────────────────────────────────────────────────────────
  1.   Design Complete      01-Apr-26       20       DONE
  2.   Backend API          15-Apr-26       40       IN_PROGRESS
  3.   Testing              30-Apr-26       35       NOT_STARTED
  ────────────────────────────────────────────────────────
  Total: 95 SP   |   Completed: 20 SP   |   Remaining: 75 SP

  1. Add Milestone
  2. Update Milestone Status
  3. Back

Enter option: _
```

> **V4 changes:** Added `Story Pts` column. Added SP summary line above menu options.
> Project name fetched and shown in header. Milestone list is 1-based numbered (no raw ID).

**Sub-flow – Add Milestone (option 1):**
```
  Milestone Title  : _
  Due Date         : (DD-MM-YYYY) _
  Story Points     : _

  Milestone added. ✓
```

**Sub-flow – Update Milestone Status (option 2):**
```
  Enter Milestone # : _
  New Status        : (1) NOT_STARTED   (2) IN_PROGRESS   (3) DONE

  Milestone updated. ✓
```

> **V4 change:** Update prompt now uses numbered status choices and 1-based milestone `#` instead of raw Milestone ID.

---

### ManageUsersScreen
`PrmClient/UI/Admin/ManageUsersScreen.cs`

```
Role: Admin
Date: Sunday, June 1, 2026

Manage Users

  1. Add New User
  2. List All Users
  3. Deactivate User
  4. Activate User
  5. Force Password Reset
  6. Back

Enter option: _
```

---

### SystemConfigScreen
`PrmClient/UI/Admin/SystemConfigScreen.cs`

```
Role: Admin
Date: Sunday, June 1, 2026

System Config

  1. View All Config
  2. Set Config Value
  3. Back

Enter option: _
```

**Sub-flow – View All Config (option 1):**
```
  Key                            Value
  ------------------------------ ----------------------------------------
  AI_PROVIDER                    Gemini
  MAX_ALLOCATION_PCT             100
```

**Sub-flow – Set Config Value (option 2):**
```
  Config Key: _
  Config Value: _
```

---

### ViewAllocationsScreen
`PrmClient/UI/Admin/ViewAllocationsScreen.cs`

```
Role: Admin
Date: Sunday, June 1, 2026

View Allocations

  ID    Employee   Project    %     From         To           Active
  ----- ---------- ---------- ----- ------------ ------------ ------
  1     3          5          80    2026-01-01   2026-12-31   Yes
  2     7          5          50    2026-03-01   2026-09-30   Yes

  1. Back

Enter option: _
```

---

## Manager Screens

### ManagerMenu
`PrmClient/UI/Manager/ManagerMenu.cs`

```
Role: Manager
Date: Sunday, June 1, 2026

  Manager Panel

  1. Resource Dashboard
  2. Allocate Resource
  3. My Projects
  4. Timesheets
  5. AI Assistant
  6. Logout

  Enter option: _
```

---

### ResourceDashboardScreen
`PrmClient/UI/Manager/ResourceDashboardScreen.cs`

```
Role: Manager
Date: Sunday, June 1, 2026

  Resource Dashboard

  ── ON BENCH ──────────────────────────────────────────────────────────────

  ID    Full Name                 Email                          Department      Designation          Status
  ───── ───────────────────────── ────────────────────────────── ─────────────── ──────────────────── ────────────
  4     Jane Smith                jane@example.com               Engineering     Developer            On Bench

  ── ACTIVE EMPLOYEES ──────────────────────────────────────────────────────

  ID    Full Name                 Email                          Department      Designation          Status
  ───── ───────────────────────── ────────────────────────────── ─────────────── ──────────────────── ────────────
  3     John Doe                  john@example.com               Engineering     Senior Developer     Active

  1. Drill into employee details
  2. Back

  Enter option: _
```

---

### AllocateResourceScreen
`PrmClient/UI/Manager/AllocateResourceScreen.cs`

```
Role: Manager
Date: Sunday, June 1, 2026

  Allocate Resource

  1. AI-Assisted Search
  2. Direct Allocation
  3. End Allocation
  4. Back

  Enter option: _
```

**Sub-flow – AI-Assisted Search (option 1):**
```
  ── AI-Assisted Search ─────────────────────────────────────────────────────

  Skill Requirement Description: _
  Max Hours (leave blank to skip): _
```

**Sub-flow – Direct Allocation (option 2):**
```
  ── Direct Allocation ──────────────────────────────────────────────────────

  Project ID:        _
  Employee ID:       _
  Utilisation % (1-100): _
  Start Date (yyyy-MM-dd): _
  End Date   (yyyy-MM-dd): _
```

**Sub-flow – End Allocation (option 3):**
```
  ── End Allocation ─────────────────────────────────────────────────────────

  Project ID: _
```

---

### MyProjectsScreen
`PrmClient/UI/Manager/MyProjectsScreen.cs`

```
Role: Manager
Date: Sunday, June 1, 2026

  My Projects

  ID    Name                 Description          Start Date   End Date     Status      Health
  ───── ──────────────────── ──────────────────── ──────────── ──────────── ─────────── ──────
  5     Alpha Project        Core platform build  2026-01-01   2026-12-31   ACTIVE      Green

  1. View project details & milestones
  2. Back

  Enter option: _
```

---

### ManagerTimesheetsScreen
`PrmClient/UI/Manager/ManagerTimesheetsScreen.cs`

> **V4 change:** Screen is now strictly **read-only**. The numbered menu (View / Missing / Back)
> and `ApproveRejectMenu()` are fully removed. Data is now filtered to the manager's own team
> (via `GET /api/employees/by-manager/{userId}`).

```
Role: Manager
Date: Sunday, June 1, 2026

  Filter by week (DD-MM-YYYY) or press Enter for current week: 12-May-2026
  Week: 12-May-2026

  ──────────────────────────────────────────────
  Employee           Project            Hrs     Status
  ──────────────────────────────────────────────
  Ravi Kumar         Alpha Portal       18      SUBMITTED
  Ravi Kumar         Beta CRM           20      SUBMITTED
  Anil Mehta         Gamma Rewrite       0      MISSED ⚠
  ──────────────────────────────────────────────

  [V] View employee timesheet detail     [B] Back
```

> If a timesheet's status is `MISSED`, it is displayed as `MISSED ⚠` in the Status column.
> Pressing `[V]` prompts for an employee name substring to drill into that employee's week.

---

### AIAssistantScreen
`PrmClient/UI/Manager/AIAssistantScreen.cs`

```
Role: Manager
Date: Sunday, June 1, 2026

  AI Assistant – Project Risk Summary

  Project ID: _

  <AI-generated risk summary text is displayed here>

  Press Enter to return to menu...
```

---

## Employee Screens

### EmployeeMenu
`PrmClient/UI/Employee/EmployeeMenu.cs`

> **V4 change:** On each render, the menu checks `GET /api/timesheets/employee/{userId}` for the
> previous Monday. If no record exists for that week, a reminder banner is shown above the options.
> The banner disappears once a timesheet has been submitted (any status, including MISSED).

```
Role: Employee
Date: Sunday, June 1, 2026

  ⚠  Reminder: Timesheet for week 06-May-2026 has not been submitted.
  ──────────────────────────────────────────────
  1. Submit Timesheet
  2. View My Timesheets
  3. View My Allocations
  4. Logout

  Enter option: _
```

*(Banner is hidden when the previous week's timesheet record exists.)*

---

### SubmitTimesheetScreen
`PrmClient/UI/Employee/SubmitTimesheetScreen.cs`

```
Role: Employee
Date: Sunday, June 1, 2026

  Submit Timesheet

  Week Start Date (yyyy-MM-dd): _

  Active projects for week of 2026-05-25:

  ── Project ID: 5 (Utilisation: 80%) ──
  Hours Worked: _
  Activity Tag IDs (comma-separated, or leave blank): _

  ✓ Timesheet submitted (ID: 12).

  Press any key to continue...
```

---

### ViewMyTimesheetsScreen
`PrmClient/UI/Employee/ViewMyTimesheetsScreen.cs`

> **V4 changes:**
> - Table is now grouped by week (one row per week), summing hours across all projects.
> - Columns are: `Week Start | Total Hrs | Status`.
> - If any entry in a week has `Status == "MISSED"`, the row displays `MISSED    ⚠`.
> - Resubmit prompt and `ResubmitTimesheet()` logic removed entirely.
> - `[V]` drills into a chosen week showing per-project breakdown.

```
Role: Employee
Date: Sunday, June 1, 2026

  Week Start      Total Hrs    Status
  ──────────────────────────────────────────────
  12-May-2026      38 hrs       SUBMITTED
  05-May-2026      40 hrs       SUBMITTED
  28-Apr-2026      35 hrs       SUBMITTED
  21-Apr-2026       0 hrs       MISSED    ⚠
  14-Apr-2026      40 hrs       SUBMITTED
  ──────────────────────────────────────────────

  [V] View week details     [B] Back
```

**Sub-flow – View week details ([V]):**
```
  Enter week start (DD-MM-YYYY): 05-05-2026

  ── Week: 05-May-2026 — Status: SUBMITTED ─────

  Project          Hrs    Status
  ────────────────────────────────────────────────────────
  Project 5        20     SUBMITTED
  Project 7        20     SUBMITTED
  ────────────────────────────────────────────────────────
  Total: 40 hrs
```

---

### ViewMyAllocationsScreen
`PrmClient/UI/Employee/ViewMyAllocationsScreen.cs`

```
Role: Employee
Date: Sunday, June 1, 2026

  My Allocations

  ID    Project ID  Utilisation %   Start Date   End Date     Active
  ───── ─────────── ─────────────── ──────────── ──────────── ───────
  1     5           80              2026-01-01   2026-12-31   Yes

  Press any key to return to menu...
```
