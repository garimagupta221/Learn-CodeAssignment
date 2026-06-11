namespace PrmServer.DTOs
{
    public class SkillMatchRequestDto
    {
        public string Requirement { get; set; }
        public int ProjectId { get; set; }
        public int? MaxHours { get; set; }
    }

    public class AiResponseDto
    {
        public string Result { get; set; }
    }
}
