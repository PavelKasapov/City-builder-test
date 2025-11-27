using VContainer;
using VContainer.Unity;
using Domain.Models;
using Domain.Events;
using Application.Services;
using Presentation.Presenters;
using Presentation.Interfaces;
using MessagePipe;
using MessagePipe.VContainer;
using Application;
using Presentation.Gameplay.Views;
using Presentation.Gameplay.Presenters;
using Infrastructure.Input;
using Infrastructure.Camera;
using Application.Interfaces;
using Infrastructure.Unity;

namespace Infrastructure
{
    public class GameInstaller : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            MessagePipeOptions options = builder.RegisterMessagePipe();

            builder.RegisterBuildCallback(container =>
            {
                GlobalMessagePipe.SetProvider(container.AsServiceProvider());
                PlaceBuildingUseCase useCase = container.Resolve<PlaceBuildingUseCase>(); // Delete after implementing real class that needs PlaceBuildingUseCase
            });

            // Domain
            builder.Register<Grid>(Lifetime.Singleton)
                   .WithParameter(32)
                   .WithParameter(32);

            // Services
            builder.Register<EconomyService>(Lifetime.Singleton);
            builder.Register<GridHighlightService>(Lifetime.Singleton);
            builder.Register<GridDataService>(Lifetime.Singleton);
            builder.Register<InputService>(Lifetime.Singleton)
                   .AsImplementedInterfaces();
            builder.Register<BuildingDataService>(Lifetime.Singleton);

            // Use Cases
            builder.Register<PlaceBuildingUseCase>(Lifetime.Singleton);


            // Events and Commands
            builder.RegisterMessageBroker<PlaceBuildingCommand>(options);
            builder.RegisterMessageBroker<BuildingPlacedEvent>(options);
            builder.RegisterMessageBroker<NotEnoughResourcesEvent>(options);

            // Presentation - Views
            builder.RegisterComponentInHierarchy<GridView>()
                   .As<IGridView>();
            builder.RegisterComponentInHierarchy<HudView>()
                   .As<IHudView>();

            // Infrastructure - Unity Components
            builder.RegisterComponentInHierarchy<UnityGridAdapter>()
            .As<IGridCoordinateConverter>();

            // Presentation - Presenters
            builder.RegisterEntryPoint<GridPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HudPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<BuildingInputPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<CameraController>(Lifetime.Singleton);
        }
    }
}
