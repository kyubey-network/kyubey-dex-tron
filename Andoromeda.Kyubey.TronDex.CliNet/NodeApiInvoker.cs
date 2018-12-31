using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.TronDex.CliNet
{
    public class NodeApiInvoker : IDisposable
    {
        private HttpClient _client = new HttpClient { BaseAddress = new Uri("https://wlcyapi.tronscan.org/") };

        public void Dispose()
        {
            _client?.Dispose();
        }

        public async Task<ContractTransationsResponse> GetContractTransactionsAsync(string contract, int skip = 0, int take = 100, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync($"/api/contracts/transaction?limit={take}&start={skip}&contract={contract}", cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ContractTransationsResponse>(responseText);
            }
        }

        public async Task<TransationResponse> GetTransactionAsync(string hash, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync($"/api/transaction/{hash}", cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TransationResponse>(responseText);
            }
        }
    }
}
