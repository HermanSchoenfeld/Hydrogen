using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using VelocityNET.Presentation.Hydrogen.Loader.Plugins;
using VelocityNET.Presentation.Hydrogen.Services;

namespace VelocityNET.Presentation.Hydrogen.Loader
{

    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            ConfigureServices(builder.Services);
            
            await builder.Build().RunAsync();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddViewModelsFromAssembly(typeof(Program).Assembly);

            serviceCollection.AddTransient<IPluginLocator, StaticPluginLocator>();
                
            serviceCollection.AddSingleton<IAppManager, DefaultAppManager>();
            serviceCollection.AddSingleton<IPluginManager, DefaultPluginManager>();
            serviceCollection.AddSingleton<IModalService, ModalService>();
            
            InitializePlugins(serviceCollection);
        }

        /// <summary>
        /// Initializes plugin system.
        /// </summary>
        /// <param name="serviceCollection"> current service collection</param>
        private static void InitializePlugins(IServiceCollection serviceCollection)
        {
            ServiceProvider provider = serviceCollection.BuildServiceProvider();
            IPluginManager manager = provider.GetRequiredService<IPluginManager>();
            
            manager.ConfigureServices(serviceCollection);
        }
    }
}