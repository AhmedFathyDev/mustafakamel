using System.ComponentModel.DataAnnotations;

namespace Portfolio.Dashboard.Models;

public class Skill
{
    [Key] public int Id { get; init; }
    
    [StringLength(30)] public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public SkillType Type { get; set; }
}

public enum SkillType
{
    Hard,
    Soft,
    Personal,
    Transferable,
    KnowledgeBased
}