using Domain.Models;
using R3;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Services
{
    public class EconomyService
    {
        private readonly Dictionary<ResourceType, ReactiveProperty<int>> _resources = new Dictionary<ResourceType, ReactiveProperty<int>>();
        private readonly Dictionary<ResourceType, ReadOnlyReactiveProperty<int>> _readOnlyResources = new Dictionary<ResourceType, ReadOnlyReactiveProperty<int>>();

        public ReadOnlyReactiveProperty<int> Gold => _readOnlyResources[ResourceType.Gold];

        public EconomyService()
        {
            _resources[ResourceType.Gold] = new ReactiveProperty<int>(3000);
            _readOnlyResources[ResourceType.Gold] = _resources[ResourceType.Gold].ToReadOnlyReactiveProperty();

            Debug.Log($"Initial gold: {Gold.CurrentValue}");
        }

        public bool CanAfford(ResourceData cost)
        {
            return _resources[cost.Type].CurrentValue >= cost.Amount;
        }

        public bool TrySpend(ResourceData cost)
        {
            if (!CanAfford(cost))
            {
                return false;
            }

            _resources[cost.Type].Value -= cost.Amount;
            return true;
        }

        public void AddResource(ResourceData gain)
        {
            _resources[gain.Type].Value += gain.Amount;
        }
    }
}
