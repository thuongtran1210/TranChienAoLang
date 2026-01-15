using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Duck Battle/Skills/Egg Bombardment")]
public class EggBombardmentSkillSO : DuckSkillSO
{
    [Min(1)]
    [SerializeField] private int _eggsCount = 5;

    [Header("Feedback")]
    [SerializeField] private string _feedbackFormat = "Egg Bombardment hit {0} cells!";
    [SerializeField] private string _noTargetMessage = "No cells left to bombard.";

    public override List<Vector2Int> GetAffectedPositions(Vector2Int pivotPos, IGridSystem targetGrid)
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        if (targetGrid == null)
        {
            return cells;
        }

        for (int x = 0; x < targetGrid.Width; x++)
        {
            for (int y = 0; y < targetGrid.Height; y++)
            {
                cells.Add(new Vector2Int(x, y));
            }
        }

        return cells;
    }

    protected override void ExecuteCore(IGridSystem targetGrid, Vector2Int pivotPos, BattleEventChannelSO eventChannel, Owner targetOwner)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 0; x < targetGrid.Width; x++)
        {
            for (int y = 0; y < targetGrid.Height; y++)
            {
                GridCell cell = targetGrid.GetCell(x, y);
                if (cell != null && !cell.IsHit)
                {
                    candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        if (candidates.Count == 0)
        {
            eventChannel.RaiseSkillFeedback(_noTargetMessage, pivotPos);
            return;
        }

        int bombsToDrop = Mathf.Min(_eggsCount, candidates.Count);
        List<Vector2Int> bombTargets = new List<Vector2Int>();

        for (int i = 0; i < bombsToDrop; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Vector2Int targetPos = candidates[index];
            candidates.RemoveAt(index);

            bombTargets.Add(targetPos);
            targetGrid.ShootAt(targetPos);
        }

        if (bombTargets.Count > 0)
        {
            ApplyVisualFeedback(bombTargets, eventChannel, targetOwner);
            string message = string.Format(_feedbackFormat, bombTargets.Count);
            eventChannel.RaiseSkillFeedback(message, pivotPos);
        }
        else
        {
            eventChannel.RaiseSkillFeedback(_noTargetMessage, pivotPos);
        }
    }
}