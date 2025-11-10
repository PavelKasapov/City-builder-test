using VContainer;
using VContainer.Unity;
using Domain.Models;
using Domain.Events;
using Application;
using Application.Services;
using Presentation.Interfaces;
using Presentation.Views;
using MessagePipe;
using Presentation.Presenters;
using System;
using UnityEngine;
using Grid = Domain.Models.Grid;

namespace Infrastructure
{
    public class GameInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[GameInstaller] Starting configuration...");

            MessagePipeOptions options = builder.RegisterMessagePipe();

            builder.RegisterBuildCallback(container =>
            {
                GlobalMessagePipe.SetProvider(container.AsServiceProvider());
                PlaceBuildingUseCase useCase = container.Resolve<PlaceBuildingUseCase>(); // Delete after implementing real class that needs PlaceBuildingUseCase
            });

            builder.Register<Grid>(Lifetime.Singleton)
                   .WithParameter(32)
                   .WithParameter(32);

            builder.Register<EconomyService>(Lifetime.Singleton);
            builder.Register<PlaceBuildingUseCase>(Lifetime.Singleton);

            builder.RegisterMessageBroker<PlaceBuildingCommand>(options);
            builder.RegisterMessageBroker<BuildingPlacedEvent>(options);
            builder.RegisterMessageBroker<NotEnoughResourcesEvent>(options);

            builder.RegisterComponentInHierarchy<GridView>()
                   .As<IGridView>();

            builder.RegisterEntryPoint<GridPresenter>(Lifetime.Singleton);
        }
    }


}
