using System.Collections.Generic;

namespace PrmServer.DTOs
{
    public class SkillMatchRequestDto
    {
        public string Requirement { get; set; }
        public int? ProjectId { get; set; }
        public int? MaxHours { get; set; }
    }

    /// <summary>
    /// Internal value object returned by IAiService methods.
    /// Carries both the LLM-generated text and the name of the provider that answered.
    /// </summary>
    public record AiResult(string Result, string Provider);

    /// <summary>
    /// HTTP response envelope returned by AiController endpoints.
    /// </summary>
    public class AiResponseDto
    {
        /// <summary>The AI-generated text.</summary>
        public string Result { get; set; }

        /// <summary>The name of the AI provider that generated the result (e.g. Gemma, Gemini, Grok).</summary>
        public string Provider { get; set; }
    }

    public class SkillMatchRecommendationDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string SkillsMatch { get; set; }
        public string Availability { get; set; }
        public string RecentActivity { get; set; }
        public string Reason { get; set; }
    }

    public class SkillMatchResponseDto
    {
        public List<SkillMatchRecommendationDto> Recommendations { get; set; }
        public string Provider { get; set; }
    }

    public record SkillMatchResult(List<SkillMatchRecommendationDto> Recommendations, string Provider);

    // ── Team Builder DTOs ────────────────────────────────────────────────────────

    /// <summary>
    /// One role the manager wants to staff — title, required skills, and minimum proficiency.
    /// </summary>
    public class TeamRoleRequest
    {
        public string RoleTitle { get; set; }
        public List<string> RequiredSkills { get; set; } = new();
        /// <summary>Beginner | Intermediate | Advanced | Expert</summary>
        public string MinProficiency { get; set; } = "Beginner";
    }

    /// <summary>
    /// Request body for POST api/ai/team-builder.
    /// Defines the project name and all roles to staff in one pass.
    /// </summary>
    public class TeamBuilderRequestDto
    {
        public string ProjectName { get; set; }
        public string TeamRequirement { get; set; }
    }

    /// <summary>
    /// Result for a single role. Either filled (Filled=true) with an employee,
    /// or a gap (Filled=false) explaining precisely why.
    /// </summary>
    public class TeamRoleResultDto
    {
        public string RoleTitle { get; set; }
        public bool Filled { get; set; }

        // Populated when Filled == true
        public int? EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string MatchedSkills { get; set; }
        public string Reason { get; set; }

        // Populated when Filled == false
        /// <summary>"NoSkill" — nobody has the skill; or "Allocated" — best match is currently allocated.</summary>
        public string GapReason { get; set; }
        /// <summary>Human-readable detail: e.g. "No active bench engineer has Java" or "Alice is allocated until 2026-08-01".</summary>
        public string GapDetail { get; set; }
    }

    /// <summary>HTTP response envelope for POST api/ai/team-builder.</summary>
    public class TeamBuilderResponseDto
    {
        public string ProjectName { get; set; }
        public List<TeamRoleResultDto> Results { get; set; } = new();
        public string Provider { get; set; }
    }

    /// <summary>Internal value object returned by AiService.BuildTeamAsync.</summary>
    public record TeamBuilderResult(string ProjectName, List<TeamRoleResultDto> Results, string Provider);
}
