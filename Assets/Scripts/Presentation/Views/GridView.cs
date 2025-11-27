using UnityEngine;
using Domain.Models;
using Presentation.Interfaces;
using Application.Services;
using VContainer;
using R3;

namespace Presentation.Gameplay.Views
{
    public class GridView : MonoBehaviour, IGridView
    {
        [SerializeField] private Material _gridMaterial;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Texture2D _cellStatesTexture;
        private GridDataService _gridDataService;
        private GridHighlightService _highlightService;

        private Color _freeColor = Color.black;
        private Color _occupiedColor = Color.red;
        private Color _hoverValidColor = Color.green;
        private Color _hoverInvalidColor = new Color(1, 0.5f, 0);

        private CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(GridDataService gridDataService, GridHighlightService highlightService)
        {
            _gridDataService = gridDataService;
            _highlightService = highlightService;
        }

        public void Initialize(int width, int height)
        {
            CreateComponents();
            CreateMesh(width, height);
            SetupTexture(width, height);
            SetupSubscriptions();
            UpdateAllCells();
        }

        private void SetupSubscriptions()
        {
            _highlightService.HoveredPosition
                .Subscribe(_ => UpdateTexture())
                .AddTo(_disposables);

            _highlightService.HoveredSize
                .Subscribe(_ => UpdateTexture())
                .AddTo(_disposables);

            _highlightService.IsPositionValid
                .Subscribe(_ => UpdateTexture())
                .AddTo(_disposables);
        }

        public void SetCellState(GridPosition position, bool isOccupied)
        {
            UpdateTexture();
        }

        private void UpdateAllCells()
        {
            UpdateTexture();
        }

        private void UpdateTexture()
        {
            foreach (GridPosition position in _gridDataService.GetAllPositions())
            {
                Color color = GetCellColor(position);
                _cellStatesTexture.SetPixel(position.X, position.Y, color);
            }
            _cellStatesTexture.Apply();
        }

        private Color GetCellColor(GridPosition position)
        {
            bool isOccupied = _gridDataService.IsCellOccupied(position);
            Color baseColor = isOccupied ? _occupiedColor : _freeColor;

            GridPosition? hoveredPosition = _highlightService.HoveredPosition.CurrentValue;
            BuildingSize? hoveredSize = _highlightService.HoveredSize.CurrentValue;

            if (hoveredPosition.HasValue && hoveredSize.HasValue)
            {
                GridPosition hoverPos = hoveredPosition.Value;
                BuildingSize size = hoveredSize.Value;

                if (position.X >= hoverPos.X && position.X < hoverPos.X + size.Width &&
                    position.Y >= hoverPos.Y && position.Y < hoverPos.Y + size.Height)
                {
                    return _highlightService.IsPositionValid.CurrentValue ? _hoverValidColor : _hoverInvalidColor;
                }
            }

            return baseColor;
        }

        private void CreateComponents()
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.material = new Material(_gridMaterial);
        }

        private void CreateMesh(int width, int height)
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(width, 0, 0),
                new Vector3(0, height, 0),
                new Vector3(width, height, 0)
            };

            int[] triangles = { 0, 2, 1, 2, 3, 1 };
            Vector2[] uv = { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            _meshFilter.mesh = mesh;
        }

        private void SetupTexture(int width, int height)
        {
            _cellStatesTexture = new Texture2D(width, height);
            _cellStatesTexture.filterMode = FilterMode.Point;

            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = _freeColor;
            }
            _cellStatesTexture.SetPixels(colors);
            _cellStatesTexture.Apply();

            _meshRenderer.material.SetTexture("_CellColors", _cellStatesTexture);
            _meshRenderer.material.SetVector("_GridSize", new Vector4(width, height, 0, 0));
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
    }
}
