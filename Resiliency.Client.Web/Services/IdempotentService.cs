using System.Net.Http;
using System.Threading.Tasks;

namespace Resiliency.Client.Services 
{
    public class IdempotentService : IIdempotentService 
    {

        private readonly HttpClient _httpClient;

        public IdempotentService(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }

        public async Task<string> Get()
        {
            return await _httpClient.GetStringAsync("api/greetings");
        }

        public async Task Create()
        {
            await _httpClient.PostAsync("api/greetings", null);
        }
    }

    public interface IIdempotentService
    {
        Task<string> Get();
        Task Create();
    }
}
