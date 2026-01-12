using UnityEngine;

public enum AIActionType
{
    NormalAttack,
    CastSkill
}

public struct AIAction
{
    public AIActionType Type;
    public Vector2Int TargetPosition;
    public DuckSkillSO SkillToCast; 

    // Factory method helper (Modern C# style)
    public static AIAction Attack(Vector2Int pos) => new AIAction
    {
        Type = AIActionType.NormalAttack,
        TargetPosition = pos
    };

    public static AIAction Skill(Vector2Int pos, DuckSkillSO skill) => new AIAction
    {
        Type = AIActionType.CastSkill,
        TargetPosition = pos,
        SkillToCast = skill
    };
}