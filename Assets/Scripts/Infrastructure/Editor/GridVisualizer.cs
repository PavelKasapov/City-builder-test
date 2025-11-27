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
            if (_grid == null)
            {
                _grid = GetComponent<UnityGrid>();
            }
        }

        private void OnDrawGizmos()
        {
            if (!_showGrid || _grid == null) return;

            Gizmos.color = _gridColor;

            for (int x = 0; x <= _width; x++)
            {
                Vector3 start = _grid.CellToWorld(new Vector3Int(x, 0, 0));
                Vector3 end = _grid.CellToWorld(new Vector3Int(x, _height, 0));
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= _height; y++)
            {
                Vector3 start = _grid.CellToWorld(new Vector3Int(0, y, 0));
                Vector3 end = _grid.CellToWorld(new Vector3Int(_width, y, 0));
                Gizmos.DrawLine(start, end);
            }

            Gizmos.color = Color.white;
            Vector3 bottomLeft = _grid.CellToWorld(new Vector3Int(0, 0, 0));
            Vector3 topLeft = _grid.CellToWorld(new Vector3Int(0, _height, 0));
            Vector3 bottomRight = _grid.CellToWorld(new Vector3Int(_width, 0, 0));
            Vector3 topRight = _grid.CellToWorld(new Vector3Int(_width, _height, 0));

            Gizmos.DrawLine(bottomLeft, topLeft);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
        }
    }
}
