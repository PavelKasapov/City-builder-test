using VContainer;
using VContainer.Unity;
using Domain;
using Application;
using Presentation;
using Presentation.Views;
using MessagePipe;
using MessagePipe.VContainer;

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
            });

            builder.Register<Grid>(Lifetime.Singleton)
                   .WithParameter(32)
                   .WithParameter(32);

            builder.Register<PlaceBuildingUseCase>(Lifetime.Singleton);

            builder.RegisterMessageBroker<BuildingPlacedEvent>(options);

            builder.RegisterComponentInHierarchy<GridView>();
            builder.RegisterEntryPoint<GridPresenter>(Lifetime.Singleton);
        }
    }
}
