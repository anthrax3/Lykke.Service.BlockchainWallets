﻿using Autofac;
using Common.Log;
using Lykke.Service.BlockchainWallets.AzureRepositories;
using Lykke.Service.BlockchainWallets.Core.Repositories;
using Lykke.Service.BlockchainWallets.Core.Settings.ServiceSettings;
using Lykke.SettingsReader;

namespace Lykke.Service.BlockchainWallets.Modules
{
    public class RepositoriesModule : Module
    {
        private readonly IReloadingManager<DbSettings> _dbSettings;
        private readonly ILog _log;

        public RepositoriesModule(
            IReloadingManager<DbSettings> dbSettings,
            ILog log)
        {
            _log = log;
            _dbSettings = dbSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var connectionString = _dbSettings.Nested(x => x.DataConnString);

            builder
                .Register(c => WalletRepository.Create(connectionString, _log))
                .As<IWalletRepository>()
                .SingleInstance();

            builder
                .Register(c => AdditionalWalletRepository.Create(connectionString, _log))
                .As<IAdditionalWalletRepository>()
                .SingleInstance();

            builder
                .Register(c => MonitoringSubscriptionRepository.Create(connectionString, _log))
                .As<IMonitoringSubscriptionRepository>()
                .SingleInstance();
        }
    }
}
