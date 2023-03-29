﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using Microsoft.Health.Fhir.Core.Features.Persistence.Orchestration;
using Microsoft.Health.Fhir.Tests.Common;
using Microsoft.Health.Test.Utilities;
using Xunit;

namespace Microsoft.Health.Fhir.Shared.Core.UnitTests.Features.Persistence.Orchestration
{
    [Trait(Traits.OwningTeam, OwningTeam.Fhir)]
    [Trait(Traits.Category, Categories.Bundle)]
    [Trait(Traits.Category, Categories.BundleOrchestrator)]
    public class BundleOrchestratorTests
    {
        private readonly object _dataLayer = new object();

        [Fact]
        public void GivenAnOrchestrator_WhenAskedForAJob_ReceiveANewJobBack()
        {
            const string label = "label";
            const int expectedNumberOfResources = 100;

            var batchOrchestrator = new BundleOrchestrator<object>(_dataLayer);
            IBundleOrchestratorOperation<object> operation = batchOrchestrator.CreateNewOperation(BundleOrchestratorOperationType.Batch, label, expectedNumberOfResources);

            Assert.Equal(label, operation.Label);
            Assert.Equal(expectedNumberOfResources, operation.OriginalExpectedNumberOfResources);

            batchOrchestrator.RemoveOperation(operation.Id);
        }

        [Fact]
        public void GivenAnOrchestrator_WhenAskedForAJobWithInvalidParameters_ReceiveArgumentExpections()
        {
            var batchOrchestrator = new BundleOrchestrator<object>(_dataLayer);

            Assert.Throws<ArgumentNullException>(() => batchOrchestrator.CreateNewOperation(BundleOrchestratorOperationType.Batch, null, expectedNumberOfResources: 100));

            Assert.Throws<ArgumentException>(() => batchOrchestrator.CreateNewOperation(BundleOrchestratorOperationType.Batch, string.Empty, expectedNumberOfResources: 100));

            Assert.Throws<ArgumentOutOfRangeException>(() => batchOrchestrator.CreateNewOperation(BundleOrchestratorOperationType.Batch, "test", expectedNumberOfResources: -1));

            Assert.Throws<ArgumentOutOfRangeException>(() => batchOrchestrator.CreateNewOperation(BundleOrchestratorOperationType.Batch, "test", expectedNumberOfResources: 0));

            Assert.Throws<BundleOrchestratorException>(() => batchOrchestrator.RemoveOperation(Guid.Empty));
        }
    }
}
