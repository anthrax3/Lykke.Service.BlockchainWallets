﻿using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Service.BlockchainWallets.Core.Services;
using Lykke.Service.BlockchainWallets.Workflow.Commands;
using Lykke.Service.BlockchainWallets.Workflow.Events;


namespace Lykke.Service.BlockchainWallets.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class BeginBalanceMonitoringCommandHandler
    {
        private readonly IBlockchainIntegrationService _blockchainIntegrationService;
        private readonly ILog _log;


        public BeginBalanceMonitoringCommandHandler(
            IBlockchainIntegrationService blockchainIntegrationService,
            ILog log)
        {
            _blockchainIntegrationService = blockchainIntegrationService;
            _log = log;
        }


        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(BeginBalanceMonitoringCommand command, IEventPublisher publisher)
        {
            _log.WriteInfo(nameof(BeginBalanceMonitoringCommand), command, "");

            try
            {
                var apiClient = _blockchainIntegrationService.TryGetApiClient(command.BlockchainType);

                if (apiClient != null)
                {
                    await apiClient.StartBalanceObservationAsync(command.Address);
                    
                    publisher.PublishEvent(new BalanceMonitoringBeganEvent
                    {
                        Address = command.Address,
                        AssetId = command.AssetId,
                        BlockchainType = command.BlockchainType
                    });

                    return CommandHandlingResult.Ok();
                }

                throw new NotSupportedException($"Blockchain type [{command.BlockchainType}] is not supported");
            }
            catch (Exception e)
            {
                _log.WriteError(nameof(BeginBalanceMonitoringCommand), command, e);

                throw;
            }
        }
    }
}
