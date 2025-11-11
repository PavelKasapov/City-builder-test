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

        public ReadOnlyReactiveProperty<int> Gold => this._readOnlyResources[ResourceType.Gold];

        public EconomyService()
        {
            this._resources[ResourceType.Gold] = new ReactiveProperty<int>(3000);
            this._readOnlyResources[ResourceType.Gold] = this._resources[ResourceType.Gold].ToReadOnlyReactiveProperty();

            Debug.Log($"Initial gold: {this.Gold.CurrentValue}");
        }

        public bool CanAfford(ResourceData cost)
        {
            return this._resources[cost.Type].CurrentValue >= cost.Amount;
        }

        public bool TrySpend(ResourceData cost)
        {
            if (!this.CanAfford(cost))
            {
                return false;
            }

            this._resources[cost.Type].Value -= cost.Amount;
            return true;
        }

        public void AddResource(ResourceData gain)
        {
            this._resources[gain.Type].Value += gain.Amount;
        }
    }
}
