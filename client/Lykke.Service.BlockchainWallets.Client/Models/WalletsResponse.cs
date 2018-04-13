﻿using System.Collections.Generic;

namespace Lykke.Service.BlockchainWallets.Client.Models
{
    /// <summary>
    ///     All clients wallets.
    /// </summary>
    public class WalletsResponse
    {
        public IEnumerable<WalletResponse> Wallets { get; set; }

        public string ContinuationToken { get; set; }
    }
}
