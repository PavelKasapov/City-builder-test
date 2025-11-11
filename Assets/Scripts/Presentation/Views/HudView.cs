using UnityEngine;
using UnityEngine.UIElements;
using Presentation.Interfaces;
using R3;
using Domain.Models;

namespace Presentation.Gameplay.Views
{
    public class HudView : MonoBehaviour, IHudView
    {
        [SerializeField]
        private UIDocument _uiDocument;

        private Label _goldLabel;
        private Button _houseButton;
        private Button _farmButton;
        private Button _mineButton;

        private readonly Subject<BuildingType> _buildingSelectedSubject = new Subject<BuildingType>();
        public Observable<BuildingType> OnBuildingSelected => this._buildingSelectedSubject;

        public void Initialize()
        {
            // Решаем проблему с инициализацией через активацию/деактивацию
            this._uiDocument.enabled = false;
            this._uiDocument.enabled = true;

            this.InitializeUI();
        }

        private void InitializeUI()
        {
            VisualElement root = this._uiDocument.rootVisualElement;

            this._goldLabel = root.Q<Label>("gold-label");
            this._houseButton = root.Q<Button>("house-button");
            this._farmButton = root.Q<Button>("farm-button");
            this._mineButton = root.Q<Button>("mine-button");

            this._houseButton.focusable = false;
            this._farmButton.focusable = false;
            this._mineButton.focusable = false;

            if (this._houseButton != null)
                this._houseButton.clicked += () => this.OnBuildingButtonClicked(BuildingType.House);

            if (this._farmButton != null)
                this._farmButton.clicked += () => this.OnBuildingButtonClicked(BuildingType.Farm);

            if (this._mineButton != null)
                this._mineButton.clicked += () => this.OnBuildingButtonClicked(BuildingType.Mine);
        }

        public void UpdateGoldDisplay(int goldAmount)
        {
            if (this._goldLabel != null)
            {
                this._goldLabel.text = $"Gold: {goldAmount}";
            }
        }

        private void OnBuildingButtonClicked(BuildingType buildingType)
        {
            this._buildingSelectedSubject.OnNext(buildingType);
        }

        private void OnDestroy()
        {
            this._buildingSelectedSubject?.Dispose();
        }
    }
}
