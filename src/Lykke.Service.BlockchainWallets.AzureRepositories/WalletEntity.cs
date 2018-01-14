﻿using System;
using Lykke.AzureStorage.Tables;
using Lykke.Service.BlockchainWallets.Core.Domain.Wallet;

namespace Lykke.Service.BlockchainWallets.AzureRepositories
{
    public class WalletEntity : AzureTableEntity, IWallet
    {
        public string Address { get; set; }

        public string AssetId { get; set; }

        public string IntegrationLayerId { get; set; }

        public Guid ClientId { get; set; }


        public static string GetPartitionKey(string integrationLayerId, string assetId)
            => $"{integrationLayerId}-{assetId}";

        public static string GetRowKey(Guid clientId)
            => $"{clientId:N}";
    }
}
