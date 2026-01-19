using System;
using Domain.Models;
using Presentation.Interfaces;
using Application.Services;
using UnityEngine;
using VContainer;
using R3;

namespace Presentation.Gameplay.Views
{
    public class GridView : MonoBehaviour, IGridView
    {
        [SerializeField] private Material _gridMaterial;

        [SerializeField] private Color _freeColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        [SerializeField] private Color _occupiedColor = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color _hoverValidColor = new Color(0f, 1f, 0f, 0.8f);
        [SerializeField] private Color _hoverInvalidColor = new Color(1f, 0f, 0f, 0.8f);

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Texture2D _gridDataTexture; // Текстура данных: R=занятость, G=наведение
        private GridDataService _gridDataService;
        private GridHighlightService _highlightService;

        private CompositeDisposable _disposables = new CompositeDisposable();
        private Material _materialInstance;

        private int _width;
        private int _height;

        // Кэши
        private Color[] _pixelCache; // Кэш цветов пикселей
        private bool[] _occupancyCache; // Кэш занятости

        // Отслеживание наведения (используем RectInt вместо 4 переменных)
        private RectInt? _previousHoverRect = null;

        // Batching
        private bool _pendingApply;
        private bool _textureNeedsFullUpdate;

        [Inject]
        public void Construct(GridDataService gridDataService, GridHighlightService highlightService)
        {
            _gridDataService = gridDataService;
            _highlightService = highlightService;
        }

        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;

            CreateComponents();
            CreateMesh(width, height);
            SetupTextures(width, height);
            SetupSubscriptions();
            UpdateGridTexture();
        }

        private void SetupSubscriptions()
        {
            _highlightService.HoveredPosition
                .Subscribe(_ => UpdateGridTexture())
                .AddTo(_disposables);

            _highlightService.HoveredSize
                .Subscribe(_ => UpdateGridTexture())
                .AddTo(_disposables);

            _highlightService.IsPositionValid
                .Subscribe(_ => UpdateGridTexture())
                .AddTo(_disposables);
        }

        public void SetCellState(GridPosition position, bool isOccupied)
        {
            if (_gridDataTexture == null || _occupancyCache == null || _pixelCache == null)
                return;

            int x = position.X;
            int y = position.Y;
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return;

            int index = y * _width + x;

            // Проверяем, изменилось ли состояние
            if (_occupancyCache[index] == isOccupied)
                return;

            // Обновляем кэш занятости
            _occupancyCache[index] = isOccupied;

            // Обновляем цвет пикселя
            float occ = isOccupied ? 1f : 0f;
            float hov = _pixelCache[index].g; // Сохраняем текущее состояние наведения
            Color newColor = new Color(occ, hov, 0f, 1f);

            if (_pixelCache[index] != newColor)
            {
                _pixelCache[index] = newColor;
                _gridDataTexture.SetPixel(x, y, newColor);
                _pendingApply = true;
            }
        }

        private void UpdateGridTexture()
        {
            if (_gridDataTexture == null || _occupancyCache == null || _pixelCache == null)
                return;

            // Получаем текущие данные о наведении
            GridPosition? hoveredPosition = _highlightService.HoveredPosition.CurrentValue;
            BuildingSize? hoveredSize = _highlightService.HoveredSize.CurrentValue;

            // Вычисляем текущий прямоугольник наведения
            RectInt? currentHoverRect = null;
            if (hoveredPosition.HasValue && hoveredSize.HasValue)
            {
                GridPosition hp = hoveredPosition.Value;
                BuildingSize hs = hoveredSize.Value;

                int startX = Mathf.Clamp(hp.X, 0, _width - 1);
                int startY = Mathf.Clamp(hp.Y, 0, _height - 1);
                int endX = Mathf.Clamp(hp.X + hs.Width - 1, 0, _width - 1);
                int endY = Mathf.Clamp(hp.Y + hs.Height - 1, 0, _height - 1);

                if (startX <= endX && startY <= endY)
                {
                    currentHoverRect = new RectInt(startX, startY,
                        endX - startX + 1,
                        endY - startY + 1);
                }
            }

            // Если нет ни текущего, ни предыдущего наведения
            if (!_previousHoverRect.HasValue && !currentHoverRect.HasValue)
            {
                // Только применяем ожидающие изменения в LateUpdate
                return;
            }

            // Вычисляем область, которую нужно обновить (объединение предыдущего и текущего)
            RectInt updateRect;
            if (_previousHoverRect.HasValue && currentHoverRect.HasValue)
            {
                RectInt prev = _previousHoverRect.Value;
                RectInt curr = currentHoverRect.Value;

                int minX = Mathf.Min(prev.xMin, curr.xMin);
                int minY = Mathf.Min(prev.yMin, curr.yMin);
                int maxX = Mathf.Max(prev.xMax - 1, curr.xMax - 1);
                int maxY = Mathf.Max(prev.yMax - 1, curr.yMax - 1);

                updateRect = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
            }
            else if (_previousHoverRect.HasValue)
            {
                updateRect = _previousHoverRect.Value;
            }
            else // currentHoverRect.HasValue
            {
                updateRect = currentHoverRect.Value;
            }

            // Обрезаем область обновления до границ текстуры
            updateRect.xMin = Mathf.Clamp(updateRect.xMin, 0, _width - 1);
            updateRect.yMin = Mathf.Clamp(updateRect.yMin, 0, _height - 1);
            updateRect.width = Mathf.Clamp(updateRect.width, 0, _width - updateRect.xMin);
            updateRect.height = Mathf.Clamp(updateRect.height, 0, _height - updateRect.yMin);

            bool anyChange = false;

            // Обходим область обновления
            for (int y = updateRect.yMin; y < updateRect.yMax; y++)
            {
                for (int x = updateRect.xMin; x < updateRect.xMax; x++)
                {
                    int index = y * _width + x;

                    // Определяем, находится ли ячейка в текущей области наведения
                    bool isInCurrentHover = currentHoverRect.HasValue &&
                                           currentHoverRect.Value.Contains(new Vector2Int(x, y));

                    float occ = _occupancyCache[index] ? 1f : 0f;
                    float hov = isInCurrentHover ? 1f : 0f;
                    Color newColor = new Color(occ, hov, 0f, 1f);

                    // Обновляем только если цвет изменился
                    if (_pixelCache[index] != newColor)
                    {
                        _pixelCache[index] = newColor;
                        _gridDataTexture.SetPixel(x, y, newColor);
                        anyChange = true;
                    }
                }
            }

            if (anyChange)
            {
                _pendingApply = true;
            }

            // Сохраняем текущее наведение как предыдущее
            _previousHoverRect = currentHoverRect;
        }

        /// <summary>
        /// Обновляет кэш занятости из сервиса (оптимизированная версия)
        /// </summary>
        public void RefreshOccupancyCacheFromService()
        {
            if (_occupancyCache == null || _pixelCache == null || _gridDataTexture == null)
                return;

            bool anyChange = false;

            // Пытаемся получить все данные одним вызовом
            // Предполагаем, что GridDataService имеет метод GetAllOccupancies
            // Если нет, придется делать по одной ячейке (медленно!)

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = y * _width + x;
                    bool occupied = _gridDataService.IsCellOccupied(new GridPosition(x, y));

                    if (_occupancyCache[index] != occupied)
                    {
                        _occupancyCache[index] = occupied;

                        float occ = occupied ? 1f : 0f;
                        float hov = _pixelCache[index].g; // Сохраняем наведение
                        Color newColor = new Color(occ, hov, 0f, 1f);

                        _pixelCache[index] = newColor;
                        _gridDataTexture.SetPixel(x, y, newColor);
                        anyChange = true;
                    }
                }
            }

            if (anyChange)
            {
                _pendingApply = true;
            }
        }

        /// <summary>
        /// Обновляет занятость только для указанных ячеек
        /// </summary>
        public void RefreshCellsFromService(System.Collections.Generic.IEnumerable<GridPosition> positions)
        {
            if (_occupancyCache == null || _pixelCache == null || _gridDataTexture == null)
                return;

            bool anyChange = false;

            foreach (var pos in positions)
            {
                int x = pos.X;
                int y = pos.Y;

                if (x < 0 || x >= _width || y < 0 || y >= _height)
                    continue;

                int index = y * _width + x;
                bool occupied = _gridDataService.IsCellOccupied(pos);

                if (_occupancyCache[index] != occupied)
                {
                    _occupancyCache[index] = occupied;

                    float occ = occupied ? 1f : 0f;
                    float hov = _pixelCache[index].g;
                    Color newColor = new Color(occ, hov, 0f, 1f);

                    _pixelCache[index] = newColor;
                    _gridDataTexture.SetPixel(x, y, newColor);
                    anyChange = true;
                }
            }

            if (anyChange)
            {
                _pendingApply = true;
            }
        }

        private void CreateComponents()
        {
            _meshFilter = gameObject.AddComponent<MeshFilter>();
            _meshRenderer = gameObject.AddComponent<MeshRenderer>();

            _materialInstance = new Material(_gridMaterial);
            _meshRenderer.material = _materialInstance;
            _meshRenderer.enabled = true;

            if (_materialInstance != null)
            {
                _materialInstance.renderQueue = 3000; // Transparent

                _materialInstance.SetColor("_FreeColor", _freeColor);
                _materialInstance.SetColor("_OccupiedColor", _occupiedColor);
                _materialInstance.SetColor("_HoverValidColor", _hoverValidColor);
                _materialInstance.SetColor("_HoverInvalidColor", _hoverInvalidColor);
            }
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

        private void SetupTextures(int width, int height)
        {
            // Используем оптимальный формат: RGB24 (3 байта на пиксель)
            // Можно было бы использовать RG16, но его поддержка не везде есть
            _gridDataTexture = new Texture2D(width, height, TextureFormat.RG16, false);
            _gridDataTexture.filterMode = FilterMode.Point; // Без интерполяции
            _gridDataTexture.wrapMode = TextureWrapMode.Clamp; // Не повторять текстуру

            // Инициализируем кэши
            _pixelCache = new Color[width * height];
            _occupancyCache = new bool[width * height];

            // Заполняем кэши начальными данными
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    bool occupied = _gridDataService.IsCellOccupied(new GridPosition(x, y));

                    _occupancyCache[index] = occupied;
                    float occ = occupied ? 1f : 0f;
                    Color c = new Color(occ, 0f, 0f, 1f); // Начально без наведения

                    _pixelCache[index] = c;
                    pixels[index] = c;
                }
            }

            // Загружаем пиксели в текстуру
            _gridDataTexture.SetPixels(pixels);
            _gridDataTexture.Apply(false); // false = не генерировать мипмапы

            // Устанавливаем текстуру и параметры в материал
            if (_materialInstance != null)
            {
                _materialInstance.SetTexture("_GridData", _gridDataTexture);
                _materialInstance.SetVector("_GridSize", new Vector4(width, height, 0, 0));
            }
        }

        private void LateUpdate()
        {
            // Применяем все накопленные изменения один раз за кадр
            if (_pendingApply && _gridDataTexture != null)
            {
                _gridDataTexture.Apply(false);
                _pendingApply = false;
            }
        }

        private void OnDestroy()
        {
            _disposables?.Dispose();

            // Очищаем созданные объекты
            if (_materialInstance != null)
            {
                Destroy(_materialInstance);
            }

            if (_gridDataTexture != null)
            {
                Destroy(_gridDataTexture);
            }

            if (_meshFilter != null && _meshFilter.mesh != null)
            {
                Destroy(_meshFilter.mesh);
            }
        }
    }
}
