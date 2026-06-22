using PrmClient.Models;
using PrmClient.Services;

namespace PrmClient.UI.Manager
{
    public class AIAssistantScreen : IScreen
    {
        private readonly ApiClient _api;

        public AIAssistantScreen(ApiClient api)
        {
            _api = api;
        }

        public void Render()
        {
            while (true)
            {
                ConsoleHelper.ClearScreen();
                ConsoleHelper.PrintHeader(AppState.Role);
                Console.WriteLine("  AI Assistant");
                Console.WriteLine();
                Console.WriteLine("  1. Skill Match    — Find best employees for a project requirement");
                Console.WriteLine("  2. Risk Summary   — Get a health analysis for a project");
                Console.WriteLine("  3. Team Builder   — Staff a whole project team in one search (bench only)");
                Console.WriteLine("  4. Back");
                Console.WriteLine();

                int choice = InputHelper.GetValidIntOption("  Enter option: ", 1, 4);

                switch (choice)
                {
                    case 1: SkillMatchFlow();   break;
                    case 2: RiskSummaryFlow();  break;
                    case 3: TeamBuilderFlow();  break;
                    case 4:
                        AppState.CurrentScreen = "manager-menu";
                        return;
                }
            }
        }

        private void SkillMatchFlow()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  ── Skill Match ────────────────────────────────");
            Console.WriteLine();

            try
            {
                string requirement = InputHelper.GetRequiredString("  Describe your project requirement in plain English: ");

                Console.WriteLine("\n  Searching... (calling AI)");

                var dto = new SkillMatchRequestDto
                {
                    Requirement = requirement,
                    ProjectId   = null,
                    MaxHours    = null
                };

                var result = _api.PostAsync<SkillMatchRequestDto, SkillMatchResponseDto>("api/ai/skill-match", dto)
                                 .GetAwaiter().GetResult();

                if (result == null || result.Recommendations == null || result.Recommendations.Count == 0)
                {
                    Console.WriteLine("\n  No employees found matching your requirement.");
                    Console.WriteLine("  Try broadening your search or check employee skill profiles.");
                }
                else
                {
                    Console.WriteLine("\n  AI-MATCHED RESULTS");
                    Console.WriteLine("  ─────────────────────────────────────────────────────────────────────────────────────────────────────────");
                    Console.WriteLine($"    {"Rank",-6} {"Name of Engineer",-25} {"Allocation %",-15} {"Reason"}");
                    Console.WriteLine("  ─────────────────────────────────────────────────────────────────────────────────────────────────────────");

                    for (int i = 0; i < result.Recommendations.Count; i++)
                    {
                        var rec         = result.Recommendations[i];
                        string rank     = $"#{i + 1}";
                        var reasonLines = WrapText(rec.Reason, 60);

                        Console.WriteLine($"    {rank,-6} {rec.FullName,-25} {rec.Availability,-15} {reasonLines[0]}");
                        for (int j = 1; j < reasonLines.Count; j++)
                        {
                            Console.WriteLine(new string(' ', 53) + reasonLines[j]);
                        }
                    }
                    Console.WriteLine("  ─────────────────────────────────────────────────────────────────────────────────────────────────────────");
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.Write("  [B] Back > ");
            Console.ReadLine();
        }

        private void RiskSummaryFlow()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  ── Risk Summary ───────────────────────────────");
            Console.WriteLine();

            try
            {
                var projects = _api.GetAsync<List<ProjectModel>>($"api/projects/manager/{AppState.UserId}").GetAwaiter().GetResult() ?? new List<ProjectModel>();
                if (projects.Count == 0)
                {
                    Console.WriteLine("  No projects assigned to you.");
                    Console.WriteLine("\n  Press any key to continue...");
                    Console.ReadKey(intercept: true);
                    return;
                }

                Console.WriteLine("  Your Projects:");
                foreach (var p in projects)
                {
                    Console.WriteLine($"    {p.Id} {p.Name} ({p.Health})");
                }
                Console.WriteLine();

                int projectId = InputHelper.GetValidIntOption("  Enter project number (ID): ", 1, int.MaxValue);

                Console.WriteLine("\n  Generating AI summary...");

                var result = _api.GetAsync<AiResponseDto>($"api/ai/risk-summary/{projectId}")
                                 .GetAwaiter().GetResult();

                Console.WriteLine();
                Console.WriteLine(result?.Result);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.Write("  Press Enter to return...");
            Console.ReadLine();
        }

        private void TeamBuilderFlow()
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.PrintHeader(AppState.Role);
            Console.WriteLine("  ── Team Builder ───────────────────────────────────────────────────────────");
            Console.WriteLine("  Define your whole project team at once. Only 100% bench employees are shown.");
            Console.WriteLine("  Managers see all employees company-wide.");
            Console.WriteLine();

            try
            {
                string projectName = InputHelper.GetRequiredString("  Project name: ");
                string teamReq     = InputHelper.GetRequiredString("  Team requirements (e.g. 2 Java developers, 1 DevOps, 1 QA): ");

                Console.WriteLine();
                Console.WriteLine($"  Searching for best bench employees for '{projectName}'... (calling AI)");

                var request = new TeamBuilderRequestModel
                {
                    ProjectName     = projectName,
                    TeamRequirement = teamReq
                };

                var result = _api.PostAsync<TeamBuilderRequestModel, TeamBuilderResponseModel>(
                    "api/ai/team-builder", request).GetAwaiter().GetResult();

                if (result == null || result.Results == null || result.Results.Count == 0)
                {
                    Console.WriteLine("\n  No results returned. Please try again.");
                }
                else
                {
                    RenderTeamBuilderResults(result);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"\n  Error: {ex.Message}");
            }

            Console.WriteLine();
            Console.Write("  Press Enter to return...");
            Console.ReadLine();
        }

        private static void RenderTeamBuilderResults(TeamBuilderResponseModel result)
        {
            Console.WriteLine();
            Console.WriteLine($"  TEAM BUILDER RESULTS — {result.ProjectName}");
            Console.WriteLine($"  Powered by: {result.Provider}");
            Console.WriteLine();

            int filledCount = 0;
            int gapCount    = 0;

            foreach (var r in result.Results)
            {
                if (r.Filled)
                {
                    filledCount++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  ✅ {r.RoleTitle}");
                    Console.ResetColor();
                    Console.WriteLine($"       Assigned to : {r.EmployeeName}");
                    Console.WriteLine($"       Skills match: {r.MatchedSkills}");
                    var reasonLines = WrapText(r.Reason, 70);
                    Console.WriteLine($"       Reason      : {reasonLines[0]}");
                    for (int j = 1; j < reasonLines.Count; j++)
                        Console.WriteLine(new string(' ', 20) + reasonLines[j]);
                }
                else
                {
                    gapCount++;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"  ❌ {r.RoleTitle}  [{r.GapReason}]");
                    Console.ResetColor();
                    var detailLines = WrapText(r.GapDetail, 75);
                    Console.WriteLine($"       Why         : {detailLines[0]}");
                    for (int j = 1; j < detailLines.Count; j++)
                        Console.WriteLine(new string(' ', 20) + detailLines[j]);
                }
                Console.WriteLine();
            }

            Console.WriteLine("  ─────────────────────────────────────────────────────────────────────────");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  Summary: {filledCount} role(s) filled  |  {gapCount} gap(s) remaining");
            Console.ResetColor();

            if (gapCount > 0)
            {
                Console.WriteLine();
                Console.WriteLine("  Gap legend:");
                Console.WriteLine("    NoSkill          — Nobody in the company has this skill. Hire or train.");
                Console.WriteLine("    Allocated        — Best match exists but is currently on a project.");
                Console.WriteLine("                       Check the date shown and plan around it.");
                Console.WriteLine("    NoAvailableBench — Only qualified person was already assigned above.");
            }
        }

        private static List<string> WrapText(string text, int maxWidth)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string> { "" };
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            var currentLine = "";

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 > maxWidth)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        lines.Add(currentLine);
                        currentLine = word;
                    }
                    else
                    {
                        lines.Add(word);
                        currentLine = "";
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(currentLine))
                        currentLine = word;
                    else
                        currentLine += " " + word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines;
        }
    }
}
