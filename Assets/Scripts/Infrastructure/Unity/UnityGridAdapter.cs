using UnityEngine;
using Domain.Gameplay.Models;
using UnityGrid = UnityEngine.Grid; // Псевдоним для избежания конфликта

namespace Infrastructure.Unity
{
    public class UnityGridAdapter : MonoBehaviour
    {
        [SerializeField]
        private UnityGrid _unityGrid; // Встроенный Unity Grid

        public Vector3 GridToWorldPosition(GridPosition gridPosition)
        {
            return this._unityGrid.CellToWorld(new Vector3Int(gridPosition.X, gridPosition.Y, 0));
        }

        public GridPosition WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3Int cellPosition = this._unityGrid.WorldToCell(worldPosition);
            return new GridPosition(cellPosition.x, cellPosition.y);
        }
    }
}
