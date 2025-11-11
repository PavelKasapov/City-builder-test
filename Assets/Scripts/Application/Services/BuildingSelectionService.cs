using Domain.Models;
using R3;

namespace Application.Services
{
    public class BuildingSelectionService
    {
        public ReactiveProperty<BuildingType?> SelectedBuilding { get; } = new ReactiveProperty<BuildingType?>();
    }
}
