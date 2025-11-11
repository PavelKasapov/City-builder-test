using Presentation.Interfaces;
using Domain.Events;
using MessagePipe;
using R3;
using VContainer;
using Application;
using Domain.Models;
using VContainer.Unity;
using UnityEngine;

namespace Presentation.Presenters
{
    public class BuildingInputPresenter : IInitializable, System.IDisposable
    {
        private readonly IHudView _hudView;
        private readonly PlaceBuildingUseCase _placeBuildingUseCase;
        private readonly IPublisher<PlaceBuildingCommand> _commandPublisher;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public BuildingInputPresenter(
            IHudView hudView,
            PlaceBuildingUseCase placeBuildingUseCase,
            IPublisher<PlaceBuildingCommand> commandPublisher)
        {
            this._hudView = hudView;
            this._placeBuildingUseCase = placeBuildingUseCase;
            this._commandPublisher = commandPublisher;
        }

        public void Initialize()
        {
            this._hudView.OnBuildingSelected.Subscribe(this.OnBuildingSelected).AddTo(this._disposables);
        }

        public void Dispose()
        {
            this._disposables?.Dispose();
        }

        private void OnBuildingSelected(BuildingType buildingType)
        {
            // TODO: Добавить реальную систему выбора места для строительства.
            this._commandPublisher.Publish(new PlaceBuildingCommand
            {
                Position = new Domain.Models.GridPosition(3, 3),
                BuildingType = buildingType
            });
        }
    }
}
