﻿using System.Threading.Tasks;

namespace Lykke.Service.BlockchainWallets.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
