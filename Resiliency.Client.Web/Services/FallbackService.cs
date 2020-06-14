using System.Net.Http;
using System.Threading.Tasks;

namespace Resiliency.Client.Services 
{
    public class FallbackService : IFallbackService {
        private readonly HttpClient _httpClient;

        public FallbackService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<string> Get() {
            return await _httpClient.GetStringAsync("api/greetings");
        }
    }

    public interface IFallbackService {
        Task<string> Get();
    }
}
