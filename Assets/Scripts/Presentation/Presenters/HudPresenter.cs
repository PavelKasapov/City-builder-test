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
            this._hudView = hudView;
            this._economyService = economyService;
        }

        public void Initialize()
        {
            this._hudView.Initialize();
            this._economyService.Gold.Subscribe(this.OnGoldChanged).AddTo(this._disposables);
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
    }
}
