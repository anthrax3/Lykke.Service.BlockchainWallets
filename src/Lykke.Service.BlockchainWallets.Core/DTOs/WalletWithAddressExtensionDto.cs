﻿namespace Lykke.Service.BlockchainWallets.Core.DTOs
{
    public class WalletWithAddressExtensionDto : WalletDto
    {
        public string AddressExtension { get; set; }

        public string BaseAddress { get; set; }
    }
}
