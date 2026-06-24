# PRM Tool - REST API Contracts

This document outlines the RESTful API contracts for the Project & Resource Management (PRM) Tool. It is designed to be easily parsed by agents, front-end developers, and automated testing tools.

## Base URL
`/api`

---

## 1. Authentication & Users (`AuthController`)
Handles JWT generation, session management, and user account creation.

| HTTP Method | Endpoint | Description | Auth | Request Body (DTO) / Parameters |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/auth/login` | Authenticates user and returns a JWT. | None | `{ "username": "str", "password": "str" }` |
| **POST** | `/auth/signup` | Creates a new account. **Admin-only** (V4). | `Admin` | `{ "fullName": "str", "email": "str", "username": "str", "password": "str", "role": "str" }` |
| **POST** | `/auth/change-password` | Updates password on first login (Admin created). | Bearer | `{ "userId": "int", "newPassword": "str" }` |
| **POST** | `/auth/logout` | Invalidates the current JWT session. | Bearer | *(no body)* |

> **V4 change:** `POST /auth/signup` is now protected by `[Authorize(Roles = "Admin")]`.
> Public self-registration is no longer permitted.

---

## 2. Employee Management (`EmployeeController`)
Handles the core workforce data, ensuring clean separation from the auth logic.

| HTTP Method | Endpoint | Description | Auth | Request Body (DTO) / Parameters |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/employees` | Retrieves all employees (Admin/Manager view). | Bearer | None |
| **GET** | `/employees/{id}` | Gets a specific employee's full profile. | Bearer | Path Param: `id` (int) |
| **GET** | `/employees/available` | Gets employees currently on the bench. | Bearer | None |
| **GET** | `/employees/by-manager/{managerUserId}` | Gets employees assigned to a specific manager. | `Manager,Admin` | Path Param: `managerUserId` (int) |
| **POST** | `/employees` | Creates a new employee profile (links to UserId). | Bearer | `{ "userId": "int", "fullName": "str", "email": "str", "department": "str", "designation": "str" }` |
| **PUT** | `/employees/{id}` | Updates employee details. | Bearer | `{ "fullName": "str", "department": "str", "designation": "str" }` |
| **PUT** | `/employees/assign-manager` | Assigns a manager to an employee. | `Admin` | `{ "employeeUserId": "int", "managerUserId": "int" }` |
| **DELETE** | `/employees/{id}` | Deactivates an employee (soft delete, cascades allocations). | Bearer | Path Param: `id` (int) |
| **POST** | `/employees/{id}/skills` | Assigns a new skill to an employee. | Bearer | `{ "skillId": "int", "proficiency": "str" }` |
| **PUT** | `/employees/{id}/skills/{skillId}` | Updates an employee's skill proficiency. | `Admin` | `{ "proficiency": "str" }` |
| **DELETE** | `/employees/{id}/skills/{skillId}` | Removes a skill from an employee. | `Admin` | Path Params: `id`, `skillId` (int) |

> **V4 additions:** `GET /employees/by-manager/{managerUserId}` and `PUT /employees/assign-manager` are new.
> `Employee.ManagerUserId` (nullable FK to `Users.Id`) is a new column on the `Employees` table.

---

## 3. Project & Milestone Management (`ProjectController`, `MilestoneController`)
Manages project lifecycles, health tracking, story points, and milestone progression.

| HTTP Method | Endpoint | Description | Auth | Request Body (DTO) / Parameters |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/projects` | Gets all projects (Admin view). Returns `ProjectSummaryDto` including `CompletedStoryPoints`. | Bearer | None |
| **GET** | `/projects/{id}` | Gets a single project by ID. | Bearer | Path Param: `id` (int) |
| **GET** | `/projects/manager/{id}` | Gets projects assigned to a specific manager. Returns `ProjectSummaryDto`. | Bearer | Path Param: `id` (int) |
| **POST** | `/projects` | Creates a new project. | Bearer | `{ "name": "str", "description": "str", "startDate": "YYYY-MM-DD", "endDate": "YYYY-MM-DD", "managerId": "int", "totalStoryPoints": "int" }` |
| **PUT** | `/projects/{id}` | Updates all project details. | Bearer | `{ "name": "str", "description": "str", "startDate": "YYYY-MM-DD", "endDate": "YYYY-MM-DD", "status": "str(PLANNED\|ACTIVE\|ON_HOLD\|COMPLETED)", "managerId": "int", "totalStoryPoints": "int" }` |
| **GET** | `/projects/{id}/health` | Gets current health flag (GREEN / AMBER / RED). | Bearer | Path Param: `id` (int) |
| **POST** | `/projects/{id}/milestones` | Adds a milestone to a project. | Bearer | `{ "title": "str", "dueDate": "YYYY-MM-DD", "storyPoints": "int" }` |
| **GET** | `/milestones/project/{projectId}` | Gets all milestones for a project. Returns `storyPoints` field. | Bearer | Path Param: `projectId` (int) |
| **PUT** | `/milestones/{id}` | Updates milestone status. | Bearer | `{ "status": "str(NOT_STARTED\|IN_PROGRESS\|DONE)" }` |

> **V4 additions/changes:**
> - `GET /projects/{id}` is a new endpoint.
> - `POST /projects` and `PUT /projects/{id}` now include `totalStoryPoints`.
> - `PUT /projects/{id}` now accepts the full set of editable fields (previously only `name`, `status`, `endDate`); `COMPLETED` is a valid status.
> - `POST /projects/{id}/milestones` now accepts `storyPoints`.
> - `GET /projects` and `GET /projects/manager/{id}` return `ProjectSummaryDto` which includes `totalStoryPoints` and `completedStoryPoints` (calculated server-side from milestones with `Status == "DONE"`).
> - `Project.TotalStoryPoints` and `Milestone.StoryPoints` are new DB columns (EF migration: `AddStoryPoints`).

---

## 4. Resource Allocation (`AllocationController`)
Handles the assignment of employees to projects, enforcing utilization limits.

| HTTP Method | Endpoint | Description | Auth | Request Body (DTO) / Parameters |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/allocations/employee/{id}` | Gets all allocations for a specific employee. | Bearer | Path Param: `id` (int) |
| **GET** | `/allocations/project/{id}` | Gets all allocated resources for a specific project. | Bearer | Path Param: `id` (int) |
| **POST** | `/allocations` | Allocates an employee to a project (validates capacity). | Bearer | `{ "employeeId": "int", "projectId": "int", "utilizationPct": "int(1-100)", "startDate": "YYYY-MM-DD", "endDate": "YYYY-MM-DD" }` |
| **PUT** | `/allocations/{id}/end` | Ends an active allocation immediately (sets endDate to today). | Bearer | Path Param: `id` (int) |

---

## 5. Timesheets (`TimesheetController`)
Manages the weekly logging of hours and activity tags.

| HTTP Method | Endpoint | Description | Auth | Request Body (DTO) / Parameters |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/timesheets/employee/{id}` | Gets timesheet history for an employee. Includes `MISSED` records created by scheduler. | Bearer | Path Param: `id` (int) |
| **POST** | `/timesheets` | Submits a new weekly timesheet. | Bearer | `{ "employeeId": "int", "projectId": "int", "weekStart": "YYYY-MM-DD", "hoursLogged": "float", "tagIds": "[int]" }` |
| **PUT** | `/timesheets/{id}` | Updates/resubmits a rejected timesheet (server-side, not exposed in V4 console). | Bearer | `{ "hoursLogged": "float", "tagIds": "[int]" }` |
| **PUT** | `/timesheets/{id}/approve` | Approves a timesheet (server-side; no longer accessible from console in V4). | Bearer | Path Param: `id` (int) |
| **PUT** | `/timesheets/{id}/reject` | Rejects a timesheet (server-side; no longer accessible from console in V4). | Bearer | `{ "reason": "str" }` |
| **GET** | `/timesheets/missing-current-week` | Gets employees who have no timesheet for the current week. | `Manager,Admin` | None |

> **V4 note:** `PUT /timesheets/{id}`, `PUT /timesheets/{id}/approve`, and `PUT /timesheets/{id}/reject`
> still exist server-side but are no longer called from the console client. The manager timesheets
> screen is now read-only. The employee timesheets screen no longer offers resubmission.

---

## 6. AI Assistant (`AiController`)
Provides natural-language skill matching and project risk analysis, scoped to the calling manager's team.

| HTTP Method | Endpoint | Description | Auth | Request Body (DTO) / Parameters |
| :--- | :--- | :--- | :--- | :--- |
| **POST** | `/ai/skill-match` | Returns AI-ranked employee suggestions for a requirement. **Scoped to caller's team (V4).** | `Manager,Admin` | `{ "requirement": "str", "projectId": "int", "maxHours": "int?" }` |
| **GET** | `/ai/risk-summary/{projectId}` | Returns AI-generated project health risk summary. | `Manager,Admin` | Path Param: `projectId` (int) |

> **V4 change:** `POST /ai/skill-match` now reads the caller's user ID from the JWT `sub` claim
> and filters the employee pool to `WHERE ManagerUserId = callerUserId`. Only the calling
> manager's assigned team members are considered by the AI.

---

## 7. System Configuration (`SystemConfigController`)
Handles dynamic system settings written to the local configuration file.

| HTTP Method | Endpoint | Description | Auth | Request Body (DTO) / Parameters |
| :--- | :--- | :--- | :--- | :--- |
| **GET** | `/config` | Retrieves the current system configuration. | Bearer | None |
| **PUT** | `/config` | Updates specific configuration values. | Bearer | `{ "key": "str", "value": "str" }` |

---

## Database Schema Changes (V4 Migrations)

| Migration | Column(s) Added | Table |
| :--- | :--- | :--- |
| `AddManagerUserIdToEmployee` | `ManagerUserId INT NULL` (FK → `Users.Id`, `Restrict`) | `Employees` |
| `AddStoryPoints` | `TotalStoryPoints INT NOT NULL DEFAULT 0` | `Projects` |
| `AddStoryPoints` | `StoryPoints INT NOT NULL DEFAULT 0` | `Milestones` |

---
*Updated for V4 BRD — Phases 1–4.*
