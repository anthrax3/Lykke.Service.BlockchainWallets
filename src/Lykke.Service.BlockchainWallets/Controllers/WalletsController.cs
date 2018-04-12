﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.BlockchainWallets.Contract;
using Lykke.Service.BlockchainWallets.Core.Services;
using Lykke.Service.BlockchainWallets.Models;
using Microsoft.AspNetCore.Mvc;


namespace Lykke.Service.BlockchainWallets.Controllers
{
    [Route("api/wallets")]
    public class WalletsController : Controller
    {
        private const string RouteSuffix = "{blockchainType}/{integrationLayerAssetId}";
        private readonly IBlockchainIntegrationService _blockchainIntegrationService;
        private readonly IWalletService _walletService;


        public WalletsController(
            IBlockchainIntegrationService blockchainIntegrationService,
            IWalletService walletService)
        {
            _walletService = walletService;
            _blockchainIntegrationService = blockchainIntegrationService;
        }


        [HttpPost(RouteSuffix + "/by-client-ids/{clientId}")]
        public async Task<IActionResult> CreateWallet([FromRoute] string blockchainType, [FromRoute] string integrationLayerAssetId, [FromRoute] Guid clientId, [FromQuery] WalletType? walletType)
        {
            if (!ValidateRequest(blockchainType, integrationLayerAssetId, clientId, out var badRequest))
            {
                return badRequest;
            }

            if (!await _blockchainIntegrationService.AssetIsSupportedAsync(blockchainType, integrationLayerAssetId))
            {
                return BadRequest
                (
                    ErrorResponse.Create($"Asset [{integrationLayerAssetId}] or/and blockchain type [{blockchainType}] is not supported.")
                );
            }

            if (await _walletService.DefaultWalletExistsAsync(blockchainType, integrationLayerAssetId, clientId))
            {
                return StatusCode
                (
                    (int)HttpStatusCode.Conflict,
                    ErrorResponse.Create($"Wallet for specified client [{clientId}] has already been created.")
                );
            }

            var wallet = await _walletService.CreateWalletAsync(blockchainType, integrationLayerAssetId, clientId);

            return Ok(new WalletResponse
            {
                Address = wallet.Address,
                AddressExtension = wallet.AddressExtension,
                BlockchainType = wallet.BlockchainType,
                ClientId = wallet.ClientId,
                IntegrationLayerId = wallet.AssetId,
                IntegrationLayerAssetId = wallet.BlockchainType
            });
        }

        [HttpDelete(RouteSuffix + "/by-client-ids/{clientId}")]
        public async Task<IActionResult> DeleteWallet([FromRoute] string blockchainType, [FromRoute] string integrationLayerAssetId, [FromRoute] Guid clientId)
        {
            if (!ValidateRequest(blockchainType, integrationLayerAssetId, clientId, out var badRequest))
            {
                return badRequest;
            }

            if (!await _blockchainIntegrationService.AssetIsSupportedAsync(blockchainType, integrationLayerAssetId))
            {
                return BadRequest
                (
                    ErrorResponse.Create($"Asset [{integrationLayerAssetId}] or/and blockchain type [{blockchainType}] is not supported.")
                );
            }

            if (!await _walletService.WalletExistsAsync(blockchainType, integrationLayerAssetId, clientId))
            {
                return NotFound
                (
                    ErrorResponse.Create($"Wallet for specified client [{clientId}] does not exist.")
                );
            }

            await _walletService.DeleteWalletsAsync(blockchainType, integrationLayerAssetId, clientId);

            return Accepted();
        }
        
        [HttpGet(RouteSuffix + "/by-client-ids/{clientId}/address")]
        public async Task<IActionResult> GetAddress([FromRoute] string blockchainType, [FromRoute] string integrationLayerAssetId, [FromRoute] Guid clientId)
        {
            if (!ValidateRequest(blockchainType, integrationLayerAssetId, clientId, out var badRequest))
            {
                return badRequest;
            }

            var address = await _walletService.TryGetDefaultAddressAsync(blockchainType, integrationLayerAssetId, clientId);

            if (address != null)
            {
                return Ok(new AddressResponse
                {
                    Address = address.Address,
                    AddressExtension = address.AddressExtension
                });
            }
            else
            {
                return NoContent();
            }
        }

        [HttpGet(RouteSuffix + "/by-addresses/{address}/client-id")]
        public async Task<IActionResult> GetClientId([FromRoute] string blockchainType, [FromRoute] string integrationLayerAssetId, [FromRoute] string address)
        {
            if (!ValidateRequest(blockchainType, integrationLayerAssetId, address, out var badRequest))
            {
                return badRequest;
            }

            var clientId = await _walletService.TryGetClientIdAsync(blockchainType, integrationLayerAssetId, address);

            if (clientId != null)
            {
                return Ok(new ClientIdResponse
                {
                    ClientId = clientId.Value
                });
            }
            else
            {
                return NoContent();
            }
        }

        [HttpGet("all/by-client-ids/{clientId}")]
        public async Task<IActionResult> GetWallets([FromRoute] Guid clientId, [FromQuery] int take, [FromQuery] string continuationToken)
        {
            if (!ValidateRequest(clientId, out var badRequest))
            {
                return badRequest;
            }

            var (wallets, token) = await _walletService.GetClientWalletsAsync(clientId, take, continuationToken);

            var response = new ClientWalletsResponse
            {
                Wallets = wallets.Select(x => new WalletResponse
                {
                    Address = x.Address,
                    AddressExtension = x.AddressExtension,
                    BlockchainType = x.BlockchainType,
                    ClientId = x.ClientId,
                    IntegrationLayerId = x.AssetId,
                    IntegrationLayerAssetId = x.BlockchainType
                }),
                ContinuationToken = token
            };

            if (response.Wallets.Any())
            {
                return Ok(response);
            }
            else
            {
                return NoContent();
            }
        }


        private bool ValidateRequest(string blockchainType, string integrationLayerAssetId, string address, out IActionResult badRequest)
        {
            var invalidInputParams = new List<string>();

            if (string.IsNullOrWhiteSpace(blockchainType))
            {
                invalidInputParams.Add(nameof(blockchainType));
            }

            if (string.IsNullOrWhiteSpace(integrationLayerAssetId))
            {
                invalidInputParams.Add(nameof(integrationLayerAssetId));
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                invalidInputParams.Add(nameof(address));
            }

            if (!invalidInputParams.Any())
            {
                badRequest = null;

                return true;
            }

            badRequest = BadRequest
            (
                ErrorResponse.Create($"One or more input parameters [{string.Join(", ", invalidInputParams)}] are invalid.")
            );

            return false;
        }

        private bool ValidateRequest(string blockchainType, string integrationLayerAssetId, Guid clientId, out IActionResult badRequest)
        {
            var invalidInputParams = new List<string>();

            if (string.IsNullOrWhiteSpace(blockchainType))
            {
                invalidInputParams.Add(nameof(blockchainType));
            }

            if (string.IsNullOrWhiteSpace(integrationLayerAssetId))
            {
                invalidInputParams.Add(nameof(integrationLayerAssetId));
            }

            if (clientId == Guid.Empty)
            {
                invalidInputParams.Add(nameof(clientId));
            }

            if (!invalidInputParams.Any())
            {
                badRequest = null;

                return true;
            }

            badRequest = BadRequest
            (
                ErrorResponse.Create($"One or more input parameters [{string.Join(", ", invalidInputParams)}] are invalid.")
            );

            return false;
        }

        private bool ValidateRequest(Guid clientId, out IActionResult badRequest)
        {
            var invalidInputParams = new List<string>();

            if (clientId == Guid.Empty)
            {
                invalidInputParams.Add(nameof(clientId));
            }

            if (!invalidInputParams.Any())
            {
                badRequest = null;

                return true;
            }

            badRequest = BadRequest
            (
                ErrorResponse.Create($"One or more input parameters [{string.Join(", ", invalidInputParams)}] are invalid.")
            );

            return false;
        }
    }
}
