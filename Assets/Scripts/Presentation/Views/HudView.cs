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
        public Observable<BuildingType> OnBuildingSelected => _buildingSelectedSubject;

        public void Initialize()
        {
            // Решаем проблему с инициализацией через активацию/деактивацию
            _uiDocument.enabled = false;
            _uiDocument.enabled = true;

            InitializeUI();
        }

        private void InitializeUI()
        {
            VisualElement root = _uiDocument.rootVisualElement;

            _goldLabel = root.Q<Label>("gold-label");
            _houseButton = root.Q<Button>("house-button");
            _farmButton = root.Q<Button>("farm-button");
            _mineButton = root.Q<Button>("mine-button");

            _houseButton.focusable = false;
            _farmButton.focusable = false;
            _mineButton.focusable = false;

            if (_houseButton != null)
                _houseButton.clicked += () => OnBuildingButtonClicked(BuildingType.House);

            if (_farmButton != null)
                _farmButton.clicked += () => OnBuildingButtonClicked(BuildingType.Farm);

            if (_mineButton != null)
                _mineButton.clicked += () => OnBuildingButtonClicked(BuildingType.Mine);
        }

        public void UpdateGoldDisplay(int goldAmount)
        {
            if (_goldLabel != null)
            {
                _goldLabel.text = $"Gold: {goldAmount}";
            }
        }

        private void OnBuildingButtonClicked(BuildingType buildingType)
        {
            _buildingSelectedSubject.OnNext(buildingType);
        }

        private void OnDestroy()
        {
            _buildingSelectedSubject?.Dispose();
        }
    }
}
