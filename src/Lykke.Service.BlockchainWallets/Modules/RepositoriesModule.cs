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
            builder
                .Register(c => WalletRepository.Create(_dbSettings.Nested(x => x.DataConnString), _log))
                .As<IWalletRepository>()
                .SingleInstance();

            builder
                .Register(c => AdditionalWalletRepository.Create(_dbSettings.Nested(x => x.DataConnString), _log))
                .As<IAdditionalWalletRepository>()
                .SingleInstance();

            builder
                .Register(c => BcnCredentialsWalletRepository.Create(_dbSettings.Nested(x => x.BcnCredentialsConnString), _log))
                .As<IBcnCredentialsWalletRepository>()
                .SingleInstance();
        }
    }
}
