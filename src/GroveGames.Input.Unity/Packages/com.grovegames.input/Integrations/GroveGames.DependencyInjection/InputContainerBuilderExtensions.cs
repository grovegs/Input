using GroveGames.DependencyInjection;

namespace GroveGames.Input.Unity
{
    public static class InputContainerBuilderExtensions
    {
        public static IContainerBuilder AddPointerDispatcher(this IContainerBuilder builder)
        {
            builder.AddSingleton<UnityPointerDispatcher>();
            builder.AddSingleton<IPointerDispatcher>(resolver => resolver.Resolve<UnityPointerDispatcher>());
            return builder.AddSingleton<PointerDispatcherUpdater>();
        }

        public static IContainerBuilder AddKeyboardDispatcher(this IContainerBuilder builder)
        {
            builder.AddSingleton<UnityKeyboardDispatcher>();
            builder.AddSingleton<IKeyboardDispatcher>(resolver => resolver.Resolve<UnityKeyboardDispatcher>());
            return builder.AddSingleton<KeyboardDispatcherUpdater>();
        }
    }
}
