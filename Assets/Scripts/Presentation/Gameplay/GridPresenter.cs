using Domain;
using MessagePipe;
using Presentation.Views;
using R3;
using VContainer;
using Application;
using VContainer.Unity;

namespace Presentation
{
    public class GridPresenter : IInitializable, System.IDisposable
    {
        private readonly Grid _grid;
        private readonly GridView _gridView;
        private readonly ISubscriber<BuildingPlacedEvent> _buildingSubscriber;
        private readonly PlaceBuildingUseCase _placeBuildingUseCase;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public GridPresenter(
            Grid grid,
            GridView gridView,
            ISubscriber<BuildingPlacedEvent> buildingSubscriber,
            PlaceBuildingUseCase placeBuildingUseCase)
        {
            this._grid = grid;
            this._gridView = gridView;
            this._buildingSubscriber = buildingSubscriber;
            this._placeBuildingUseCase = placeBuildingUseCase;
        }

        public void Initialize()
        {
            this._gridView.Initialize(this._grid.Width, this._grid.Height);
            this._buildingSubscriber.Subscribe(this.OnBuildingPlaced).AddTo(this._disposables);

            // Тестовые здания через UseCase
            this._placeBuildingUseCase.Execute(new GridPosition(5, 5), BuildingType.House);
            this._placeBuildingUseCase.Execute(new GridPosition(10, 10), BuildingType.Farm);
            this._placeBuildingUseCase.Execute(new GridPosition(15, 15), BuildingType.Mine);
        }

        public void Dispose()
        {
            this._disposables?.Dispose();
        }

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            this._gridView.SetCellState(evt.Position, evt.IsOccupied);
        }
    }
}
