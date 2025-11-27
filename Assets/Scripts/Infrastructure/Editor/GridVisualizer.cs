using UnityEngine;
using UnityGrid = UnityEngine.Grid;

namespace Infrastructure.Editor
{
    [ExecuteInEditMode]
    public class GridVisualizer : MonoBehaviour
    {
        [SerializeField] private UnityGrid _grid;
        [SerializeField] private int _width = 32;
        [SerializeField] private int _height = 32;
        [SerializeField] private Color _gridColor = Color.gray;
        [SerializeField] private bool _showGrid = true;

        private void Awake()
        {
            if (this._grid == null)
            {
                this._grid = this.GetComponent<UnityGrid>();
            }
        }

        private void OnDrawGizmos()
        {
            if (!this._showGrid || this._grid == null) return;

            Gizmos.color = this._gridColor;

            for (int x = 0; x <= this._width; x++)
            {
                Vector3 start = this._grid.CellToWorld(new Vector3Int(x, 0, 0));
                Vector3 end = this._grid.CellToWorld(new Vector3Int(x, this._height, 0));
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= this._height; y++)
            {
                Vector3 start = this._grid.CellToWorld(new Vector3Int(0, y, 0));
                Vector3 end = this._grid.CellToWorld(new Vector3Int(this._width, y, 0));
                Gizmos.DrawLine(start, end);
            }

            Gizmos.color = Color.white;
            Vector3 bottomLeft = this._grid.CellToWorld(new Vector3Int(0, 0, 0));
            Vector3 topLeft = this._grid.CellToWorld(new Vector3Int(0, this._height, 0));
            Vector3 bottomRight = this._grid.CellToWorld(new Vector3Int(this._width, 0, 0));
            Vector3 topRight = this._grid.CellToWorld(new Vector3Int(this._width, this._height, 0));

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }
    }
}
