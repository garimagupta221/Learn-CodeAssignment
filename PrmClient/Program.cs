using PrmClient;
using PrmClient.Models;
using PrmClient.Services;
using PrmClient.UI;
using PrmClient.UI.Admin;
using PrmClient.UI.Common;
using PrmClient.UI.Employee;
using PrmClient.UI.Manager;

var api = new ApiClient("http://localhost:5022/");

while (AppState.CurrentScreen != "exit")
{
    if (AppState.CurrentScreen == "logout")
    {
        try
        {
            api.PostAsync<object>("api/auth/logout", new { }).GetAwaiter().GetResult();
        }
        catch { /* best-effort logout */ }
        api.ClearToken();
        AppState.Role          = "Guest";
        AppState.UserId        = 0;
        AppState.CurrentScreen = "start";
        continue;
    }

    IScreen screen = AppState.CurrentScreen switch
    {
        "start"              => new StartScreen(),
        "login"              => new LoginScreen(api),
        "change-password"    => new ChangePasswordScreen(api),
        "admin-menu"         => new AdminMenu(api),
        "admin-employees"    => new ManageEmployeesScreen(api),
        "admin-projects"     => new ManageProjectsScreen(api),
        "admin-milestones"   => new ManageMilestonesScreen(api),
        "admin-allocations"  => new ViewAllocationsScreen(api),
        "admin-users"        => new ManageUsersScreen(api),
        "admin-config"       => new SystemConfigScreen(api),
        "manager-menu"                => new ManagerMenu(api),
        "manager-dashboard"           => new ResourceDashboardScreen(api),
        "manager-allocate"            => new AllocateResourceScreen(api),
        "manager-projects"            => new MyProjectsScreen(api),
        "manager-timesheets"          => new ManagerTimesheetsScreen(api),
        "manager-ai"                  => new AIAssistantScreen(api),
        "manager-frozen-timesheets"   => new UnfreezeTimesheetScreen(api),
        "employee-menu"               => new EmployeeMenu(api),
        "employee-submit-timesheet"   => new SubmitTimesheetScreen(api),
        "employee-timesheets"         => new ViewMyTimesheetsScreen(api),
        "employee-allocations"        => new ViewMyAllocationsScreen(api),
        _                    => new StartScreen()
    };

    screen.Render();
}

