using Domain.Models;

namespace Presentation.Interfaces
{
    public interface IGridView
    {
        void Initialize(int width, int height);
        void SetCellState(GridPosition position, bool isOccupied);
    }
}
