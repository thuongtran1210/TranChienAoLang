// _Scripts/Models/GameStateBase.cs
using UnityEngine;

public abstract class GameStateBase
{
    protected IGameContext _gameContext; // Context chung (GameManager)

    public GameStateBase(IGameContext context)
    {
        _gameContext = context;
    }

    public abstract void EnterState();
    public abstract void ExitState();
    public abstract void OnGridInteraction(IGridContext source, Vector2Int gridPos);
}