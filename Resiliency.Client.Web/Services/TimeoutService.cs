using System.Net.Http;
using System.Threading.Tasks;

namespace Resiliency.Client.Services 
{
    public class TimeoutService : ITimeoutService
    {

        private readonly HttpClient _httpClient;

        public TimeoutService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task Get()
        {
            await _httpClient.GetAsync("api/greetings/sleepy-greetings");
        }
    }

    public interface ITimeoutService {
        Task Get();
    }
}
