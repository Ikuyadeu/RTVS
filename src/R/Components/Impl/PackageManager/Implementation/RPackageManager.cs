﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Newtonsoft.Json.Linq;

namespace Microsoft.R.Components.PackageManager.Implementation {
    internal class RPackageManager : IRPackageManager {
        private readonly IRInteractiveWorkflow _interactiveWorkflow;
        private readonly Action _dispose;

        public IRPackageManagerVisualComponent VisualComponent { get; private set; }
        public IRPackageManagerViewModel _viewModel;

        public RPackageManager(IRInteractiveWorkflow interactiveWorkflow, Action dispose) {
            _interactiveWorkflow = interactiveWorkflow;
            _dispose = dispose;
        }

        public IRPackageManagerVisualComponent GetOrCreateVisualComponent(IRPackageManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0) {
            if (VisualComponent != null) {
                return VisualComponent;
            }

            VisualComponent = visualComponentContainerFactory.GetOrCreate(this, instanceId).Component;
            return VisualComponent;
        }

        public async Task<IReadOnlyList<RPackage>> GetInstalledPackagesAsync() {
            return await GetPackages(async (eval) => await eval.InstalledPackages());
        }

        public async Task<IReadOnlyList<RPackage>> GetAvailablePackagesAsync() {
            return await GetPackages(async (eval) => await eval.AvailablePackages());
        }

        private async Task<IReadOnlyList<RPackage>> GetPackages(Func<IRSessionEvaluation, Task<REvaluationResult>> fetchFunc) {
            if (!_interactiveWorkflow.RSession.IsHostRunning) {
                return new List<RPackage>().AsReadOnly();
            }

            try {
                using (var eval = await _interactiveWorkflow.RSession.BeginEvaluationAsync()) {
                    var result = await fetchFunc(eval);
                    if (result.ParseStatus != RParseStatus.OK || result.Error != null) {
                        throw new InvalidOperationException(result.ToString());
                    }
                    return ((JObject)result.JsonResult).Properties()
                        .Select(p => p.Value.ToObject<RPackage>())
                        .ToList().AsReadOnly();
                }
            } catch (OperationCanceledException) {
                return new List<RPackage>().AsReadOnly();
            }
        }

        public void Dispose() {
            _dispose();
        }
    }
}