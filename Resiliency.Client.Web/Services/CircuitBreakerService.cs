using System.Net.Http;
using System.Threading.Tasks;

namespace Resiliency.Client.Services 
{
    public class CircuitBreakerService : ICircuitBreakerService 
    {
        private readonly HttpClient _httpClient;

        public CircuitBreakerService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<string> Get()
        {
            return await _httpClient.GetStringAsync("api/greetings");
        }
    }

    public interface ICircuitBreakerService 
    {
        Task<string> Get();
    }
}
