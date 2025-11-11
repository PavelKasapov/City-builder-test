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
            this._gridDataService = gridDataService;
            this._highlightService = highlightService;
        }

        public void Initialize(int width, int height)
        {
            this.CreateComponents();
            this.CreateMesh(width, height);
            this.SetupTexture(width, height);
            this.SetupSubscriptions();
            this.UpdateAllCells();
        }

        private void SetupSubscriptions()
        {
            this._highlightService.HoveredPosition
                .Subscribe(_ => this.UpdateTexture())
                .AddTo(this._disposables);

            this._highlightService.HoveredSize
                .Subscribe(_ => this.UpdateTexture())
                .AddTo(this._disposables);

            this._highlightService.IsPositionValid
                .Subscribe(_ => this.UpdateTexture())
                .AddTo(this._disposables);
        }

        public void SetCellState(GridPosition position, bool isOccupied)
        {
            this.UpdateTexture();
        }

        private void UpdateAllCells()
        {
            this.UpdateTexture();
        }

        private void UpdateTexture()
        {
            foreach (GridPosition position in this._gridDataService.GetAllPositions())
            {
                Color color = this.GetCellColor(position);
                this._cellStatesTexture.SetPixel(position.X, position.Y, color);
            }
            this._cellStatesTexture.Apply();
        }

        private Color GetCellColor(GridPosition position)
        {
            bool isOccupied = this._gridDataService.IsCellOccupied(position);
            Color baseColor = isOccupied ? this._occupiedColor : this._freeColor;

            GridPosition? hoveredPosition = this._highlightService.HoveredPosition.CurrentValue;
            BuildingSize? hoveredSize = this._highlightService.HoveredSize.CurrentValue;

            if (hoveredPosition.HasValue && hoveredSize.HasValue)
            {
                GridPosition hoverPos = hoveredPosition.Value;
                BuildingSize size = hoveredSize.Value;

                if (position.X >= hoverPos.X && position.X < hoverPos.X + size.Width &&
                    position.Y >= hoverPos.Y && position.Y < hoverPos.Y + size.Height)
                {
                    return this._highlightService.IsPositionValid.CurrentValue ? this._hoverValidColor : this._hoverInvalidColor;
                }
            }

            return baseColor;
        }

        private void CreateComponents()
        {
            this._meshFilter = this.gameObject.AddComponent<MeshFilter>();
            this._meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            this._meshRenderer.material = new Material(this._gridMaterial);
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

            this._meshFilter.mesh = mesh;
        }

        private void SetupTexture(int width, int height)
        {
            this._cellStatesTexture = new Texture2D(width, height);
            this._cellStatesTexture.filterMode = FilterMode.Point;

            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = this._freeColor;
            }
            this._cellStatesTexture.SetPixels(colors);
            this._cellStatesTexture.Apply();

            this._meshRenderer.material.SetTexture("_CellColors", this._cellStatesTexture);
            this._meshRenderer.material.SetVector("_GridSize", new Vector4(width, height, 0, 0));
        }

        private void OnDestroy()
        {
            this._disposables?.Dispose();
        }
    }
}
