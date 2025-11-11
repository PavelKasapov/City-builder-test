using Domain.Models;
using System.Collections.Generic;

namespace Application.Services
{
    public class BuildingDataService
    {
        private readonly Dictionary<BuildingType, BuildingSize> _buildingSizes = new Dictionary<BuildingType, BuildingSize>
        {
            { BuildingType.House, new BuildingSize(2, 2) },
            { BuildingType.Farm, new BuildingSize(2, 3) },
            { BuildingType.Mine, new BuildingSize(3, 3) }
        };

        private readonly Dictionary<BuildingType, ResourceData> _buildingCosts = new Dictionary<BuildingType, ResourceData>
        {
            { BuildingType.House, new ResourceData { Type = ResourceType.Gold, Amount = 100 } },
            { BuildingType.Farm, new ResourceData { Type = ResourceType.Gold, Amount = 150 } },
            { BuildingType.Mine, new ResourceData { Type = ResourceType.Gold, Amount = 200 } }
        };

        public BuildingSize GetBuildingSize(BuildingType buildingType)
        {
            return this._buildingSizes.TryGetValue(buildingType, out BuildingSize size)
                ? size
                : new BuildingSize(1, 1);
        }

        public ResourceData GetBuildingCost(BuildingType buildingType)
        {
            return this._buildingCosts.TryGetValue(buildingType, out ResourceData cost)
                ? cost
                : new ResourceData { Type = ResourceType.Gold, Amount = 0 };
        }

        public bool IsPositionValidForBuilding(Grid grid, GridPosition position, BuildingType buildingType)
        {
            BuildingSize size = this.GetBuildingSize(buildingType);

            for (int x = position.X; x < position.X + size.Width; x++)
            {
                for (int y = position.Y; y < position.Y + size.Height; y++)
                {
                    GridPosition currentPos = new GridPosition(x, y);
                    GridCell cell = grid.GetCell(currentPos);

                    if (cell == null || cell.IsOccupied)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
