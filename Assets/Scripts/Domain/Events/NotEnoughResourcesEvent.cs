using Domain.Models;

namespace Domain.Events
{
    public struct NotEnoughResourcesEvent
    {
        public ResourceType ResourceType;
    }
}
