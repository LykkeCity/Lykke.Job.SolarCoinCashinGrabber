using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Queue;
using Common;
using Lykke.Job.SolarCoinCashinGrabber.Settings.JobSettings;
using Lykke.Sdk;
using Lykke.Sdk.Health;
using Lykke.Job.SolarCoinCashinGrabber.PeriodicalHandlers;
using Lykke.Job.SolarCoinCashinGrabber.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Lykke.Job.SolarCoinCashinGrabber.Modules
{
    public class JobModule : Module
    {
        private readonly SolarCoinCashinGrabberJobSettings _settings;
        private readonly IReloadingManager<SolarCoinCashinGrabberJobSettings> _settingsManager;
        private readonly IServiceCollection _services;

        public JobModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue.SolarCoinCashinGrabberJob;
            _settingsManager = settingsManager.Nested(x => x.SolarCoinCashinGrabberJob);

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();
            
            builder.Register(ctx => AzureQueueExt.Create(_settingsManager.ConnectionString(x => x.Db.AzureQueueConnString), "solar-transit")).As<IQueueExt>().SingleInstance();

            RegisterPeriodicalHandlers(builder);
            
            RegisterMongoDb(builder);

            builder.Populate(_services);
        }

        private void RegisterMongoDb(ContainerBuilder builder)
        {
            var mongoUrl = new MongoUrl(_settings.Db.MongoConnString);

            var database = new MongoClient(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
            builder.RegisterInstance(database);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<CashinGrabber>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
        }
    }
}
