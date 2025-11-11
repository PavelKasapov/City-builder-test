using UnityEngine;
using Domain.Models;
using Presentation.Interfaces;

namespace Presentation.Gameplay.Views
{
    public class GridView : MonoBehaviour, IGridView
    {
        [SerializeField]
        private Material _gridMaterial;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Texture2D _cellStatesTexture;

        public void Initialize(int width, int height)
        {
            this.CreateComponents();
            this.CreateMesh(width, height);
            this.SetupTexture(width, height);
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
                colors[i] = Color.black;
            }

            this._cellStatesTexture.SetPixels(colors);
            this._cellStatesTexture.Apply();

            this._meshRenderer.material.SetTexture("_CellColors", this._cellStatesTexture);
            this._meshRenderer.material.SetVector("_GridSize", new Vector4(width, height, 0, 0));
        }

        public void SetCellState(GridPosition position, bool isOccupied)
        {
            Color color = isOccupied ? Color.white : Color.black;
            this._cellStatesTexture.SetPixel(position.X, position.Y, color);
            this._cellStatesTexture.Apply();
        }
    }
}
