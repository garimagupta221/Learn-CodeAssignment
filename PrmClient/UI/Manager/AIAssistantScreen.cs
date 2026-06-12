using PrmClient.Models;
using PrmClient.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;

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
                Console.WriteLine("  3. Back");
                Console.WriteLine();

                int choice = InputHelper.GetValidIntOption("  Enter option: ", 1, 3);

                switch (choice)
                {
                    case 1:
                        SkillMatchFlow();
                        break;
                    case 2:
                        RiskSummaryFlow();
                        break;
                    case 3:
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
                    Console.WriteLine("\n  No matching recommendations returned by AI.");
                }
                else
                {
                    Console.WriteLine("\n  AI-MATCHED RESULTS");
                    Console.WriteLine("  ────────────────────────────────────────────────────────────────────────────────────────────────────");
                    Console.WriteLine($"    {"Name of Engineer",-30} {"Allocation %",-15} {"Reason"}");
                    Console.WriteLine("  ────────────────────────────────────────────────────────────────────────────────────────────────────");

                    for (int i = 0; i < result.Recommendations.Count; i++)
                    {
                        var rec = result.Recommendations[i];
                        Console.WriteLine($"    {rec.FullName,-30} {rec.Availability,-15} {rec.Reason}");
                    }
                    Console.WriteLine("  ────────────────────────────────────────────────────────────────────────────────────────────────────");
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
    }
}
