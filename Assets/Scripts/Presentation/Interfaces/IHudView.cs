using R3;
using Domain.Models;

namespace Presentation.Interfaces
{
    public interface IHudView
    {
        void Initialize();
        void UpdateGoldDisplay(int goldAmount);
        Observable<BuildingType> OnBuildingSelected { get; }
    }
}
