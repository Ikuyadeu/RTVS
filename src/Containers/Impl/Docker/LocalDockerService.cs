﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.R.Common.Core.Output;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Containers.Docker {
    public abstract class LocalDockerService : IDockerService {
        public const string ContainerOutputName = "R Containers";
        private readonly IProcessServices _ps;
        private readonly IOutputService _outputService;
        private readonly Regex _containerIdMatcher64 = new Regex("[0-9a-f]{64}", RegexOptions.IgnoreCase);
        private readonly Regex _containerIdMatcher12 = new Regex("[0-9a-f]{12}", RegexOptions.IgnoreCase);
        private readonly int _defaultTimeout = 500;
        private IOutput _output;

        protected LocalDockerService(IServiceContainer services) {
            _ps = services.Process();
            _outputService = services.GetService<IOutputService>();
        }
        
        public async Task<IContainer> CreateContainerFromFileAsync(BuildImageParameters buildParams, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var buildOptions = $"-t {buildParams.Image}:{buildParams.Tag} {Path.GetDirectoryName(buildParams.DockerfilePath)}";
            await BuildImageAsync(buildOptions, ct);

            var createOptions = Invariant($"-p 5444:5444 --name {buildParams.Name} {buildParams.Image}:{buildParams.Tag} rtvsd");
            var containerId = await CreateContainerAsync(createOptions, ct);
            return await GetContainerAsync(containerId, ct);
        }

        public Task<string> BuildImageAsync(string buildOptions, CancellationToken ct) {
            var command = "build";
            return ExecuteCommandAsync(Invariant($"{command} {buildOptions}"), command, -1, true, ct);
        }

        public async Task<IEnumerable<IContainer>> ListContainersAsync(bool getAll = true, CancellationToken ct = default(CancellationToken)) {
            await TaskUtilities.SwitchToBackgroundThread();

            var command = "ps";
            var commandOptions = getAll ? "-a -q" : "-q";
            var output = await ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), null, _defaultTimeout, true, ct);
            var lines = output.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var ids = lines.Where(line => _containerIdMatcher12.IsMatch(line) || _containerIdMatcher64.IsMatch(line));
            var arr = await InspectContainerAsync(ids, ct);
            return arr.Select(c => new LocalDockerContainer(c));
        }

        public async Task<IContainer> GetContainerAsync(string containerId, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var ids = (await ListContainersAsync(true, ct)).Where(container => containerId.StartsWithIgnoreCase(container.Id));
            if (ids.Any()) {
                var arr = await InspectContainerAsync(new string[] { containerId }, ct);
                if (arr.Count == 1) {
                    return new LocalDockerContainer(arr[0]);
                }
            }

            return null;
        }

        public async Task<JArray> InspectContainerAsync(IEnumerable<string> containerIds, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var ids = containerIds.AsList();
            if (ids.Any()) {
                var command = "container inspect";
                var commandOptions = string.Join(" ", ids);
                var result = await ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), null, _defaultTimeout, false, ct);
                return JArray.Parse(result);
            }

            return new JArray();
        }

        public Task<string> RepositoryLoginAsync(string username, string password, string server, CancellationToken ct) {
            var command = "login";
            var commandOptions = $"-u {username} -p {password} {server}";
            return ExecuteCommandAsync(Invariant($"{command} {commandOptions}"), $"{command} {server}", -1, true, ct);
        }

        public Task<string> RepositoryLoginAsync(RepositoryCredentials auth, CancellationToken ct) 
            => RepositoryLoginAsync(auth.Username, auth.Password, auth.RepositoryServer, ct);

        public Task<string> RepositoryLogoutAsync(string server, CancellationToken ct) {
            var command = $"logout {server}";
            return ExecuteCommandAsync(command, command, -1, true, ct);
        }

        public Task<string> RepositoryLogoutAsync(RepositoryCredentials auth, CancellationToken ct) 
            => RepositoryLogoutAsync(auth.RepositoryServer, ct);

        public Task<string> PullImageAsync(string fullImageName, CancellationToken ct) {
            var command = $"pull {fullImageName}";
            return ExecuteCommandAsync(command, command, -1, true, ct);
        }

        public async Task<string> CreateContainerAsync(string createOptions, CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            var command = "create";
            var output = await ExecuteCommandAsync(Invariant($"{command} {createOptions}"), command, -1, true, ct);
            var matches = _containerIdMatcher64.Matches(output);

            return matches.Count >= 1 ? matches[0].Value : string.Empty;
        }

        public Task<string> DeleteContainerAsync(string containerId, CancellationToken ct) {
            var command = Invariant($"rm {containerId}");
            return ExecuteCommandAsync(command, command, -1, true, ct);
        }

        public Task<string> StartContainerAsync(string containerId, CancellationToken ct) {
            var command = Invariant($"start {containerId}");
            return ExecuteCommandAsync(command, command, -1, true, ct);
        }

        public Task<string> StopContainerAsync(string containerId, CancellationToken ct) {
            var command = Invariant($"stop {containerId}");
            return ExecuteCommandAsync(command, command, -1, true, ct);
        }

        protected abstract LocalDocker GetLocalDocker();

        private async Task<string> ExecuteCommandAsync(string arguments, string outputPrefix, int timeoutms, bool failOnTimeout = true, CancellationToken ct = default(CancellationToken)) {
            var printOutput = outputPrefix != null;
            if (_output == null && printOutput) {
                _output = await _outputService.GetAsync(ContainerOutputName, ct);
            }

            await TaskUtilities.SwitchToBackgroundThread();

            var docker = GetLocalDocker();
            var psi = new ProcessStartInfo {
                CreateNoWindow = true,
                FileName = docker.DockerCommandPath,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = _ps.Start(psi);
            var result = new StringBuilder();
            try {
                while (!process.StandardOutput.EndOfStream) {
                    var line = await process.StandardOutput.ReadLineAsync();
                    if (printOutput) {
                        _output.WriteLine(Invariant($"{outputPrefix}> {line}"));
                    }
                    result.AppendLine(line);
                }

                await process.WaitForExitAsync(timeoutms, ct);
            } catch(OperationCanceledException) when (!failOnTimeout && !ct.IsCancellationRequested){
            }

            var error = await process.StandardError.ReadToEndAsync();
            if (!string.IsNullOrEmpty(error) && !IsSecurityWarning(error)) {
                _output.Write(Invariant($"{outputPrefix}> ERROR: {error}"));
                throw new ContainerException(error);
            }
             
            return result.ToString();
        }

        private bool IsSecurityWarning(string error) {
            return error.ContainsIgnoreCase("SECURITY WARNING: You are building a Docker image from Windows against a non-Windows Docker host.");
        }
    }
}
