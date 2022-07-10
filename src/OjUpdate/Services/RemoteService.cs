using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Xylab.BricksService.OjUpdate
{
    public class RemoteServiceUpdateOrchestrator : IUpdateOrchestrator
    {
        private readonly RecordV2Storage _storage;
        private readonly HttpClient _httpClient;

        public RemoteServiceUpdateOrchestrator(RecordV2Storage storage, IUpdateDriver updateDriver, HttpClient httpClient)
        {
            _storage = storage;
            _httpClient = httpClient;
            Driver = updateDriver;
        }

        public IUpdateDriver Driver { get; }

        public async Task<IUpdateStatus> GetStatus()
        {
            return await _storage.GetStatusAsync(Driver.Category);
        }

        public async Task RequestUpdate()
        {
            using var resp = await _httpClient.PostAsync(
                "/api/OjUpdate/Trigger/" + Driver.Category,
                new ByteArrayContent(Array.Empty<byte>()));
            resp.EnsureSuccessStatusCode();
        }
    }

    public class RemoteServiceUpdateProvider : IUpdateProvider
    {
        private readonly RecordV2Storage _storage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IReadOnlyDictionary<string, IUpdateDriver> _drivers;

        public RemoteServiceUpdateProvider(RecordV2Storage storage, IHttpClientFactory httpClientFactory)
        {
            _storage = storage;
            _httpClientFactory = httpClientFactory;
            _drivers = ServiceConstants.GetDrivers().ToDictionary(k => k.Value.SiteName, v => v.Value);
        }

        public bool TryGetOrchestrator(string key, [MaybeNullWhen(false)] out IUpdateOrchestrator value)
        {
            if (_drivers.TryGetValue(key, out var driver))
            {
                value = new RemoteServiceUpdateOrchestrator(_storage, driver, _httpClientFactory.CreateClient("BricksService"));
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
