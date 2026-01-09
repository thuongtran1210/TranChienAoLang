public interface IGridContext : IGridLogic, IGridGhostHandler
{
    // Có thể thêm các member khác nếu cần thiết cho Context chung
    GridInputController InputController { get; }
}