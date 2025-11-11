using Domain.Models;
using Domain.Events;
using MessagePipe;
using Presentation.Interfaces;
using R3;
using VContainer;
using VContainer.Unity;
using Grid = Domain.Models.Grid;

namespace Presentation.Presenters
{
    public class GridPresenter : IInitializable, System.IDisposable
    {
        private readonly Grid _grid;
        private readonly IGridView _gridView;
        private readonly ISubscriber<BuildingPlacedEvent> _buildingSubscriber;
        private readonly IPublisher<PlaceBuildingCommand> _commandPublisher;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public GridPresenter(
            Grid grid,
            IGridView gridView,
            ISubscriber<BuildingPlacedEvent> buildingSubscriber,
            IPublisher<PlaceBuildingCommand> commandPublisher)
        {
            this._grid = grid;
            this._gridView = gridView;
            this._buildingSubscriber = buildingSubscriber;
            this._commandPublisher = commandPublisher;
        }

        public void Initialize()
        {
            this._gridView.Initialize(this._grid.Width, this._grid.Height);
            this._buildingSubscriber.Subscribe(this.OnBuildingPlaced).AddTo(this._disposables);

            // Both publisher test
            this._commandPublisher.Publish(new PlaceBuildingCommand
            {
                Position = new GridPosition(5, 5),
                BuildingType = BuildingType.House
            });

            IPublisher<PlaceBuildingCommand> globalPublisher = GlobalMessagePipe.GetPublisher<PlaceBuildingCommand>();
            globalPublisher.Publish(new PlaceBuildingCommand
            {
                Position = new GridPosition(10, 10),
                BuildingType = BuildingType.Farm
            });
        }

        public void Dispose()
        {
            this._disposables?.Dispose();
        }

        private void OnBuildingPlaced(BuildingPlacedEvent evt)
        {
            for (int x = evt.Position.X; x < evt.Position.X + evt.Size.Width; x++)
            {
                for (int y = evt.Position.Y; y < evt.Position.Y + evt.Size.Height; y++)
                {
                    this._gridView.SetCellState(new GridPosition(x, y), true);
                }
            }
        }
    }
}
