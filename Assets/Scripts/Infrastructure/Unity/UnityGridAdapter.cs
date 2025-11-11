using UnityEngine;
using Domain.Models;
using Application.Interfaces;
using UnityGrid = UnityEngine.Grid;

namespace Infrastructure.Unity
{
    public class UnityGridAdapter : MonoBehaviour, IGridCoordinateConverter
    {
        [SerializeField]
        private UnityGrid _unityGrid;

        private void Awake()
        {
            if (this._unityGrid == null)
            {
                this._unityGrid = this.GetComponent<UnityGrid>();
            }
        }

        public Vector3 GridToWorldPosition(GridPosition gridPosition)
        {
            // Для 2D: ручной расчет чтобы убедиться в правильности
            Vector3 worldPos = this._unityGrid.CellToWorld(new Vector3Int(gridPosition.X, gridPosition.Y, 0));

            return worldPos;
        }

        public GridPosition WorldToGridPosition(Vector3 worldPosition)
        {
            Vector3Int cellPosition = this._unityGrid.WorldToCell(worldPosition);
            GridPosition gridPosition = new GridPosition(cellPosition.x, cellPosition.y);

            return gridPosition;
        }

        public bool IsValidGridPosition(GridPosition position)
        {
            return position.X >= 0 && position.Y >= 0;
        }
    }
}
