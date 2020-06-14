using System.Net.Http;
using System.Threading.Tasks;

namespace Resiliency.Client.Services 
{
    public class WaitAndRetryService : IWaitAndRetryService 
    {
        private readonly HttpClient _httpClient;

        public WaitAndRetryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Get()
        {
            return await _httpClient.GetStringAsync("api/greetings");
        }
    }

    public interface IWaitAndRetryService {
        Task<string> Get();
    }
}
