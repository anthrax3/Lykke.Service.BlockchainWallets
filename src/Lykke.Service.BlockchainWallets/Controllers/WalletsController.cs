﻿using System;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.BlockchainWallets.Core.Services;
using Lykke.Service.BlockchainWallets.Models;
using Microsoft.AspNetCore.Mvc;


namespace Lykke.Service.BlockchainWallets.Controllers
{
    [Route("api/wallets/{integrationLayerId}/{assetId}")]
    public class WalletsController : Controller
    {
        private readonly IBlockchainIntegrationService  _blockchainIntegrationService;
        private readonly IWalletService      _walletService;


        public WalletsController(
            IBlockchainIntegrationService blockchainIntegrationService,
            IWalletService walletService)
        {
            _walletService     = walletService;
            _blockchainIntegrationService = blockchainIntegrationService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromRoute] string integrationLayerId, [FromRoute] string assetId, [FromBody] CreateWalletRequest request)
        {
            if (!await _blockchainIntegrationService.AssetIsSupported(integrationLayerId, assetId))
            {
                return NotFound();
            }
            
            if (await _walletService.WalletExistsAsync(integrationLayerId, assetId, request.ClientId))
            {
                return StatusCode((int) HttpStatusCode.Conflict);
            }

            var walletAddress = await _walletService.CreateWalletAsync(integrationLayerId, assetId, request.ClientId);

            return Ok(new WalletCreatedResponse
            {
                Address = walletAddress
            });
        }

        [HttpDelete("clients/{clientId}")]
        public async Task<IActionResult> DeleteWallet([FromRoute] string integrationLayerId, [FromRoute] string assetId, [FromRoute] Guid clientId)
        {
            if (!await _blockchainIntegrationService.AssetIsSupported(integrationLayerId, assetId))
            {
                return NotFound();
            }

            if (await _walletService.WalletExistsAsync(integrationLayerId, assetId, clientId))
            {
                return StatusCode((int)HttpStatusCode.Conflict);
            }

            await _walletService.DeleteWalletAsync(integrationLayerId, assetId, clientId);

            return Accepted();
        }

        [HttpGet("addresses/{address}/clientId")]
        public async Task<IActionResult> GetClientId([FromRoute] string integrationLayerId, [FromRoute] string assetId, [FromRoute] string address)
        {
            var clientId = await _walletService.GetClientIdAsync(integrationLayerId, assetId, address);

            if (!clientId.HasValue)
            {
                return NotFound();
            }

            return Ok(new ClientIdResponse
            {
                ClientId = clientId.Value
            });
        }
    }
}
