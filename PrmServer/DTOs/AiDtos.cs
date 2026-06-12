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
}
