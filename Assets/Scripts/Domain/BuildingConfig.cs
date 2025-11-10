using Domain.Models;

namespace Domain
{
    public struct BuildingConfig
    {
        public BuildingType Type;
        public ResourceData Cost;
        public ResourceData Income;
        public int MaxLevel;
    }
}
