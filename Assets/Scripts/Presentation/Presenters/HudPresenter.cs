using Presentation.Interfaces;
using Application.Services;
using R3;
using VContainer;
using Domain.Models;
using VContainer.Unity;
using UnityEngine;

namespace Presentation.Presenters
{
    public class HudPresenter : IInitializable, System.IDisposable
    {
        private readonly IHudView _hudView;
        private readonly EconomyService _economyService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        [Inject]
        public HudPresenter(
            IHudView hudView,
            EconomyService economyService)
        {
            this._hudView = hudView;
            this._economyService = economyService;
        }

        public void Initialize()
        {
            this._hudView.Initialize();

            // Подписываемся на изменения золота
            this._economyService.Gold.Subscribe(this.OnGoldChanged).AddTo(this._disposables);

            // Подписываемся на выбор зданий
            this._hudView.OnBuildingSelected.Subscribe(this.OnBuildingSelected).AddTo(this._disposables);

            // Устанавливаем начальное значение
            this.OnGoldChanged(this._economyService.Gold.CurrentValue);
        }

        public void Dispose()
        {
            this._disposables?.Dispose();
        }

        private void OnGoldChanged(int gold)
        {
            this._hudView.UpdateGoldDisplay(gold);
        }

        private void OnBuildingSelected(BuildingType buildingType)
        {
            Debug.Log($"[HudPresenter] Building selected in UI: {buildingType}");
            // Здесь мы будем переключать режим размещения зданий
            // Пока просто логируем
        }
    }
}
