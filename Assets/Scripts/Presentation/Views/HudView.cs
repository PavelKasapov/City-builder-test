using UnityEngine;
using UnityEngine.UIElements;
using Presentation.Interfaces;
using R3;
using Domain.Models;

namespace Presentation.Views
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
            if (this._uiDocument == null)
            {
                Debug.LogError("[HudView] UIDocument is not assigned!");
                return;
            }

            VisualElement root = this._uiDocument.rootVisualElement;

            // Находим элементы UI
            this._goldLabel = root.Q<Label>("gold-label");
            this._houseButton = root.Q<Button>("house-button");
            this._farmButton = root.Q<Button>("farm-button");
            this._mineButton = root.Q<Button>("mine-button");

            // Подписываемся на кнопки
            this._houseButton.clicked += () => this.OnBuildingButtonClicked(BuildingType.House);
            this._farmButton.clicked += () => this.OnBuildingButtonClicked(BuildingType.Farm);
            this._mineButton.clicked += () => this.OnBuildingButtonClicked(BuildingType.Mine);

            Debug.Log("[HudView] Initialized successfully");
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
            Debug.Log($"[HudView] Building selected: {buildingType}");
            this._buildingSelectedSubject.OnNext(buildingType);
        }

        private void OnDestroy()
        {
            this._buildingSelectedSubject?.Dispose();
        }
    }
}
