using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Duck Battle/Skills/Sonar Skill")]
public class SonarSkillSO : DuckSkillSO
{
    [Min(1)]
    [SerializeField] private int _radius = 1;

    [Header("Visual Feedback")]
    [Tooltip("Tile hiển thị khi PHÁT HIỆN mục tiêu")]
    [SerializeField] private TileBase _detectedIndicatorTile;

    [Tooltip("Màu hiển thị vùng quét (khi không thấy gì hoặc nền)")]
    [SerializeField] private Color _scanAreaColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [SerializeField] private string _foundMessageFormat = "Sonar detected {0} signals!";
    [SerializeField] private string _noSignalMessage = "No signals.";

    private struct SonarScanResult
    {
        public List<Vector2Int> ScanArea { get; }
        public List<Vector2Int> DetectedPositions { get; }
        public int FoundParts => DetectedPositions.Count;

        public SonarScanResult(List<Vector2Int> scanArea, List<Vector2Int> detectedPositions)
        {
            ScanArea = scanArea;
            DetectedPositions = detectedPositions;
        }
    }


    public override List<Vector2Int> GetAffectedPositions(Vector2Int pivotPos, IGridSystem targetGrid)
    {
        List<Vector2Int> area = new List<Vector2Int>();

        for (int x = -_radius; x <= _radius; x++)
        {
            for (int y = -_radius; y <= _radius; y++)
            {
                Vector2Int checkPos = pivotPos + new Vector2Int(x, y);

                if (targetGrid.IsValidPosition(checkPos))
                {
                    area.Add(checkPos);
                }
            }
        }
        return area;
    }


    private Owner ResolveTargetOwner(IGridSystem targetGrid, Owner defaultOwner)
    {
        if (targetGrid is IGridLogic gridLogic)
        {
            return gridLogic.GridOwner;
        }

        return defaultOwner;
    }

    private SonarScanResult PerformScan(IGridSystem targetGrid, Vector2Int pivotPos)
    {
        List<Vector2Int> scanArea = GetAffectedPositions(pivotPos, targetGrid);
        List<Vector2Int> detectedPositions = new List<Vector2Int>();

        foreach (var pos in scanArea)
        {
            var cell = targetGrid.GetCell(pos);
            if (cell != null && cell.OccupiedUnit != null && !cell.IsHit)
            {
                detectedPositions.Add(pos);
            }
        }

        return new SonarScanResult(scanArea, detectedPositions);
    }

    private void RaiseSonarVisuals(SonarScanResult result, BattleEventChannelSO eventChannel, Owner targetOwner, Vector2Int pivotPos)
    {
        if (result.FoundParts > 0)
        {
            if (_detectedIndicatorTile != null && result.DetectedPositions.Count > 0)
            {
                eventChannel.RaiseTileIndicator(targetOwner, result.DetectedPositions, _detectedIndicatorTile, impactDuration);
            }

            eventChannel.RaiseSkillImpactVisual(targetOwner, result.ScanArea, _scanAreaColor, impactDuration);

            string msg = string.Format(_foundMessageFormat, result.FoundParts);
            eventChannel.RaiseSkillFeedback(msg, pivotPos);
        }
        else
        {
            eventChannel.RaiseSkillImpactVisual(targetOwner, result.ScanArea, _scanAreaColor, impactDuration);
            eventChannel.RaiseSkillFeedback(_noSignalMessage, pivotPos);
        }
    }

    protected override void ExecuteCore(IGridSystem targetGrid, Vector2Int pivotPos, BattleEventChannelSO eventChannel, Owner targetOwner)
    {
        Owner actualTargetOwner = ResolveTargetOwner(targetGrid, targetOwner);
        SonarScanResult result = PerformScan(targetGrid, pivotPos);
        RaiseSonarVisuals(result, eventChannel, actualTargetOwner, pivotPos);
    }
}