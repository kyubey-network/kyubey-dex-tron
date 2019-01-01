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

        public async Task<TransactionListResponse> GetTransactionListAsync(string account, int? block = default, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync($"/api/transaction?hash={account}&block={block}", cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TransactionListResponse>(responseText);
            }
        }

        public async Task<BalanceResponse> GetBalanceAsync(string accountAddress, string accountName = default, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync($"/api/account?address={accountAddress}&name={accountName}", cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BalanceResponse>(responseText);
            }
        }

        public async Task<TransferResponse> GetTransfersAsync(string address, string token = default, string dateStart = default, string dateEnd = default, int? blockNumber = default, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync($"/api/transfer?address={address}&token={token}&date_start={dateStart}&date_to={dateEnd}&number={blockNumber}", cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TransferResponse>(responseText);
            }
        }

        public async Task<ContractResponse> GetContractAsync(string address, CancellationToken cancellationToken = default)
        {
            using (var response = await _client.GetAsync($"/api/contracts/code?contract={address}", cancellationToken))
            {
                var responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ContractResponse>(responseText);
            }
        }
    }
}
