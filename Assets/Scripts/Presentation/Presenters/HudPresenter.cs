using Presentation.Interfaces;
using Application.Services;
using R3;
using VContainer;
using VContainer.Unity;

namespace Presentation.Gameplay.Presenters
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
            _hudView = hudView;
            _economyService = economyService;
        }

        public void Initialize()
        {
            _hudView.Initialize();
            _economyService.Gold.Subscribe(OnGoldChanged).AddTo(_disposables);
            OnGoldChanged(_economyService.Gold.CurrentValue);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void OnGoldChanged(int gold)
        {
            _hudView.UpdateGoldDisplay(gold);
        }
    }
}
