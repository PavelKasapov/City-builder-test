using Domain.Models;
using UnityEngine;

namespace Application.Interfaces
{
    public interface IGridCoordinateConverter
    {
        Vector3 GridToWorldPosition(GridPosition gridPosition);
        GridPosition WorldToGridPosition(Vector3 worldPosition);
        bool IsValidGridPosition(GridPosition position);
    }
}
