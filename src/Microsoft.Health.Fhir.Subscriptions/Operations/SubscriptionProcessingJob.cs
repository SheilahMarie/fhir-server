﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Health.Fhir.Core.Features.Operations;
using Microsoft.Health.Fhir.Core.Features.Persistence;
using Microsoft.Health.Fhir.Subscriptions.Channels;
using Microsoft.Health.Fhir.Subscriptions.Models;
using Microsoft.Health.JobManagement;

namespace Microsoft.Health.Fhir.Subscriptions.Operations
{
    [JobTypeId((int)JobType.SubscriptionsProcessing)]
    public class SubscriptionProcessingJob : IJob
    {
        private readonly SubscriptionChannelFactory _storageChannelFactory;
        private readonly IFhirDataStore _dataStore;

        public SubscriptionProcessingJob(SubscriptionChannelFactory storageChannelFactory, IFhirDataStore dataStore)
        {
            _storageChannelFactory = storageChannelFactory;
            _dataStore = dataStore;
        }

        public async Task<string> ExecuteAsync(JobInfo jobInfo, CancellationToken cancellationToken)
        {
            SubscriptionJobDefinition definition = jobInfo.DeserializeDefinition<SubscriptionJobDefinition>();

            if (definition.SubscriptionInfo == null)
            {
                return HttpStatusCode.BadRequest.ToString();
            }

            var allResources = await Task.WhenAll(
                definition.ResourceReferences
                .Select(async x => await _dataStore.GetAsync(x, cancellationToken)));

            var channel = _storageChannelFactory.Create(definition.SubscriptionInfo.Channel.ChannelType);
            await channel.PublishAsync(allResources, definition.SubscriptionInfo, definition.VisibleDate, cancellationToken);

            return HttpStatusCode.OK.ToString();
        }
    }
}
