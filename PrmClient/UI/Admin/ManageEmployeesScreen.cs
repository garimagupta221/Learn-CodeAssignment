using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Admin
{
    public class ManageEmployeesScreen : IScreen
    {
        private readonly ApiClient _api;

        public ManageEmployeesScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("Manage Employees");
            Console.WriteLine();
            Console.WriteLine("  1. View All Employees");
            Console.WriteLine("  2. Update Employee");
            Console.WriteLine("  3. Deactivate Employee");
            Console.WriteLine("  4. Manage Employee Skills");
            Console.WriteLine("  5. Assign Manager");
            Console.WriteLine("  6. Back");
            Console.WriteLine();

            int choice = InputHelper.GetValidIntOption("Enter option: ", 1, 6);

            switch (choice)
            {
                case 1:
                    ListEmployees();  // owns its own render loop; returns on [B]
                    return;
                case 2: UpdateEmployee();     break;
                case 3: DeactivateEmployee(); break;
                case 4: ManageSkills();       break;
                case 5: AssignManager();      break;
                case 6:
                    AppState.CurrentScreen = "admin-menu";
                    return;
            }

            Console.WriteLine("\n  Press any key to continue...");
            Console.ReadKey(intercept: true);
        }

        // ─── List ────────────────────────────────────────────────────────────

        private void ListEmployees()
        {
            List<EmployeeModel> allEmployees;
            try
            {
                allEmployees = _api.GetAsync<List<EmployeeModel>>("api/employees")
                                   .GetAwaiter().GetResult()
                               ?? new List<EmployeeModel>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
                return;
            }

            // Active filter state (null = no filter applied)
            string? filterStatus     = null;
            string? filterDepartment = null;

            while (true)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.PrintHeader(AppState.Role);

                Console.WriteLine("╔══════════════════════════════════════════════╗");
                Console.WriteLine("║    ALL EMPLOYEES                             ║");
                Console.WriteLine("╚══════════════════════════════════════════════╝");
                Console.WriteLine();

                // Apply active filters
                var view = allEmployees.AsEnumerable();
                if (!string.IsNullOrEmpty(filterStatus))
                    view = view.Where(e => e.Status.Equals(filterStatus, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(filterDepartment))
                    view = view.Where(e => e.Department.Contains(filterDepartment, StringComparison.OrdinalIgnoreCase));

                var filtered = view.ToList();

                // Show active filter hint
                if (!string.IsNullOrEmpty(filterStatus) || !string.IsNullOrEmpty(filterDepartment))
                {
                    var parts = new List<string>();
                    if (!string.IsNullOrEmpty(filterStatus))     parts.Add($"Status: {filterStatus}");
                    if (!string.IsNullOrEmpty(filterDepartment)) parts.Add($"Department: {filterDepartment}");
                    Console.WriteLine($"  Filter active — {string.Join(", ", parts)}");
                    Console.WriteLine();
                }

                if (filtered.Count == 0)
                    Console.WriteLine("  No employees match the current filter.");
                else
                    PrintEmployeeTable(filtered);

                Console.WriteLine();
                Console.Write("  [F] Filter by Status / Department     [B] Back   > ");
                string key = (Console.ReadLine() ?? string.Empty).Trim().ToUpper();

                if (key == "B")
                    return;

                if (key == "F")
                {
                    Console.WriteLine();
                    Console.WriteLine("  Leave a field blank to match any value.");
                    Console.WriteLine();

                    Console.Write("  Status     (ALLOCATED / BENCH, or blank): ");
                    string statusInput = (Console.ReadLine() ?? string.Empty).Trim();
                    filterStatus = string.IsNullOrWhiteSpace(statusInput) ? null : statusInput;

                    Console.Write("  Department (partial match, or blank)    : ");
                    string deptInput = (Console.ReadLine() ?? string.Empty).Trim();
                    filterDepartment = string.IsNullOrWhiteSpace(deptInput) ? null : deptInput;
                }
                // Any other key — just redraw (ignore)
            }
        }

        // ─── Add ─────────────────────────────────────────────────────────────

        private void AddEmployee()
        {
            Console.WriteLine();
            Console.WriteLine("  Add Employee Profile");
            Console.WriteLine();

            int userId        = InputHelper.GetValidIntOption("  Linked User ID: ", 1, int.MaxValue);
            string fullName   = InputHelper.GetRequiredString("  Full Name: ");
            string email      = InputHelper.GetValidEmail("  Email: ");
            string department = InputHelper.GetRequiredString("  Department: ");
            string designation = InputHelper.GetRequiredString("  Designation: ");

            try
            {
                _api.PostAsync<CreateEmployeeRequest, EmployeeModel>(
                    "api/employees",
                    new CreateEmployeeRequest
                    {
                        UserId      = userId,
                        FullName    = fullName,
                        Email       = email,
                        Department  = department,
                        Designation = designation
                    }
                ).GetAwaiter().GetResult();

                Console.WriteLine("\n  Employee profile created successfully.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        // ─── Update ──────────────────────────────────────────────────────────

        private void UpdateEmployee()
        {
            Console.WriteLine();
            
            try
            {
                var allEmployees = _api.GetAsync<List<EmployeeModel>>("api/employees")
                                       .GetAwaiter().GetResult() ?? new List<EmployeeModel>();
                PrintEmployeeTable(allEmployees);
                Console.WriteLine();
            }
            catch (Exception)
            {
                // Ignore if fetch fails and proceed to prompt
            }

            int id = InputHelper.GetValidIntOption("  Employee ID to update: ", 1, int.MaxValue);

            EmployeeModel? employee;
            try
            {
                employee = _api.GetAsync<EmployeeModel>($"api/employees/{id}")
                               .GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
                Console.WriteLine("\n  Press any key to continue...");
                Console.ReadKey(intercept: true);
                return;
            }

            if (employee == null)
            {
                Console.WriteLine("\n  Employee not found.");
                Console.WriteLine("\n  Press any key to continue...");
                Console.ReadKey(intercept: true);
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"  ── {employee.FullName} (Dept: {employee.Department}, Desig: {employee.Designation}) ──");
            Console.WriteLine();

            string fullName    = InputHelper.GetRequiredString("  New Full Name: ");
            string department  = InputHelper.GetRequiredString("  New Department: ");
            string designation = InputHelper.GetRequiredString("  New Designation: ");

            try
            {
                _api.PutAsync<UpdateEmployeeRequest, EmployeeModel>(
                    $"api/employees/{id}",
                    new UpdateEmployeeRequest
                    {
                        FullName    = fullName,
                        Department  = department,
                        Designation = designation
                    }
                ).GetAwaiter().GetResult();

                Console.WriteLine("\n  Employee updated successfully.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
                Console.WriteLine("\n  Press any key to continue...");
                Console.ReadKey(intercept: true);
            }
        }

        // ─── Deactivate ───────────────────────────────────────────────────────

        private void DeactivateEmployee()
        {
            Console.WriteLine();
            int id = InputHelper.GetValidIntOption("  Employee ID to deactivate: ", 1, int.MaxValue);

            EmployeeModel? employee;
            try
            {
                employee = _api.GetAsync<EmployeeModel>($"api/employees/{id}")
                               .GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
                return;
            }

            if (employee == null)
            {
                Console.WriteLine("\n  Employee not found.");
                return;
            }

            Console.Write($"  Deactivate employee {employee.FullName} (ID: {id})? This will end all active allocations. (y/n): ");
            string confirm = Console.ReadLine()?.Trim().ToLower() ?? "n";
            if (confirm != "y")
            {
                Console.WriteLine("  Cancelled.");
                return;
            }

            try
            {
                _api.DeleteAsync($"api/employees/{id}").GetAwaiter().GetResult();
                Console.WriteLine("\n  Employee deactivated.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        // ─── Assign Skill ─────────────────────────────────────────────────────

        private void ManageSkills()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);

            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    MANAGE SKILLS                             ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();

            int empId = InputHelper.GetValidIntOption("  Enter Employee ID: ", 1, int.MaxValue);
            Console.WriteLine();

            EmployeeModel? employee;
            try
            {
                employee = _api.GetAsync<EmployeeModel>($"api/employees/{empId}")
                               .GetAwaiter().GetResult();
                if (employee == null)
                {
                    Console.WriteLine("  Employee not found.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error fetching employee: {ex.Message}");
                return;
            }

            while (true)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.PrintHeader(AppState.Role);

                Console.WriteLine("╔══════════════════════════════════════════════╗");
                Console.WriteLine("║    MANAGE SKILLS                             ║");
                Console.WriteLine("╚══════════════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine($"  Enter Employee ID: {empId}");
                Console.WriteLine();

                Console.WriteLine($"  ── {employee.FullName} ─────────────────────────────────");
                Console.WriteLine("  Current Skills:");

                List<EmployeeSkillModel> skills = new();
                try
                {
                    skills = _api.GetAsync<List<EmployeeSkillModel>>($"api/employees/{empId}/skills")
                                 .GetAwaiter().GetResult() ?? new List<EmployeeSkillModel>();
                }
                catch (Exception)
                {
                    // Ignore and show empty list
                }

                if (skills.Count == 0)
                {
                    Console.WriteLine("  (No skills assigned)");
                }
                else
                {
                    for (int i = 0; i < skills.Count; i++)
                    {
                        var s = skills[i];
                        Console.WriteLine($"  {i + 1}.  {s.SkillName,-18} {s.Proficiency}");
                    }
                }
                Console.WriteLine("  ──────────────────────────────────────────────");
                Console.WriteLine();

                Console.WriteLine("  1. Add Skill");
                Console.WriteLine("  2. Update Proficiency Level");
                Console.WriteLine("  3. Remove Skill");
                Console.WriteLine("  4. Back");
                Console.WriteLine();

                int choice = InputHelper.GetValidIntOption("  Enter option: ", 1, 4);

                if (choice == 4) break;

                switch (choice)
                {
                    case 1: AddSkill(empId); break;
                    case 2: UpdateSkill(empId, skills); break;
                    case 3: RemoveSkill(empId, skills); break;
                }

                Console.WriteLine("\n  Press any key to continue...");
                Console.ReadKey(intercept: true);
            }
        }

        private void AddSkill(int empId)
        {
            Console.WriteLine();
            string skillName = InputHelper.GetRequiredString("  Skill Name        : ");

            Console.WriteLine("  Category          : (1) Backend  (2) Frontend  (3) DevOps  (4) QA  (5) Other");
            int catChoice = InputHelper.GetValidIntOption("  Enter choice      : ", 1, 5);
            string category = catChoice switch
            {
                1 => "Backend",
                2 => "Frontend",
                3 => "DevOps",
                4 => "QA",
                _ => "Other"
            };

            Console.WriteLine("  Proficiency Level : (1) Beginner  (2) Intermediate  (3) Advanced");
            int profChoice = InputHelper.GetValidIntOption("  Enter choice      : ", 1, 3);
            string proficiency = profChoice switch
            {
                1 => "Beginner",
                2 => "Intermediate",
                _ => "Advanced"
            };

            try
            {
                _api.PostAsync<AssignSkillRequest>(
                    $"api/employees/{empId}/skills",
                    new AssignSkillRequest { SkillName = skillName, Category = category, Proficiency = proficiency }
                ).GetAwaiter().GetResult();

                Console.WriteLine("\n  Skill added. ✓");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        // ─── Update Skill ─────────────────────────────────────────────────────

        private void UpdateSkill(int empId, List<EmployeeSkillModel> skills)
        {
            if (skills.Count == 0)
            {
                Console.WriteLine("\n  No skills to update.");
                return;
            }

            Console.WriteLine();
            int index = InputHelper.GetValidIntOption("  Enter skill number to update: ", 1, skills.Count);
            int skillId = skills[index - 1].SkillId;

            Console.WriteLine("  New Proficiency Level : (1) Beginner  (2) Intermediate  (3) Advanced");
            int profChoice = InputHelper.GetValidIntOption("  Enter choice          : ", 1, 3);
            string proficiency = profChoice switch
            {
                1 => "Beginner",
                2 => "Intermediate",
                _ => "Advanced"
            };

            try
            {
                _api.PutAsync<UpdateSkillDto>(
                    $"api/employees/{empId}/skills/{skillId}",
                    new UpdateSkillDto { Proficiency = proficiency }
                ).GetAwaiter().GetResult();

                Console.WriteLine("\n  Skill updated successfully. ✓");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        // ─── Remove Skill ─────────────────────────────────────────────────────

        private void RemoveSkill(int empId, List<EmployeeSkillModel> skills)
        {
            if (skills.Count == 0)
            {
                Console.WriteLine("\n  No skills to remove.");
                return;
            }

            Console.WriteLine();
            int index = InputHelper.GetValidIntOption("  Enter skill number to remove: ", 1, skills.Count);
            int skillId = skills[index - 1].SkillId;

            try
            {
                _api.DeleteAsync($"api/employees/{empId}/skills/{skillId}").GetAwaiter().GetResult();
                Console.WriteLine("\n  Skill removed successfully. ✓");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        // ─── Assign Manager ───────────────────────────────────────────────────

        private void AssignManager()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);

            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║    ASSIGN MANAGER                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.WriteLine();

            int employeeUserId = InputHelper.GetValidIntOption("  Employee User ID : ", 1, int.MaxValue);
            int managerUserId  = InputHelper.GetValidIntOption("  Manager User ID : ", 1, int.MaxValue);

            Console.WriteLine();
            Console.WriteLine("  ──────────────────────────────────────────────");
            Console.Write("  [S] Save     [B] Back > ");

            string key = Console.ReadLine()?.Trim().ToUpper() ?? "B";
            if (key != "S")
                return;

            try
            {
                _api.PutAsync(
                    "api/employees/assign-manager",
                    new AssignManagerRequest { EmployeeUserId = employeeUserId, ManagerUserId = managerUserId }
                ).GetAwaiter().GetResult();

                Console.WriteLine("\n  Manager assigned successfully. ✓");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }
        }

        // ─── Table printer ────────────────────────────────────────────────────

        private static void PrintEmployeeTable(List<EmployeeModel> employees)
        {
            const string separator = "──────────────────────────────────────────────";

            Console.WriteLine($"  {"ID",-6}{"Name",-17}{"Department",-14}{"Status"}");
            Console.WriteLine($"  {separator}");

            foreach (var e in employees)
            {
                Console.WriteLine($"  {e.Id,-6}{e.FullName,-17}{e.Department,-14}{e.Status}");
            }

            Console.WriteLine($"  {separator}");

            int total     = employees.Count;
            int allocated = employees.Count(e => e.Status.Equals("ALLOCATED", StringComparison.OrdinalIgnoreCase));
            int bench     = employees.Count(e => e.Status.Equals("BENCH",     StringComparison.OrdinalIgnoreCase));

            Console.WriteLine($"  Total: {total}   |   Allocated: {allocated}   |   Bench: {bench}");
        }
    }
}
