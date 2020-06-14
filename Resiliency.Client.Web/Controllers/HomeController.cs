using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Resiliency.Client.Services;

namespace Resiliency.Client.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase 
    {
        private readonly IWaitAndRetryService _waitAndRetryService;
        private readonly IIdempotentService _idempotentService;
        private readonly ITimeoutService _timeoutService;
        private readonly ICircuitBreakerService _circuitBreakerService;
        private readonly IFallbackService _fallbackService;

        public HomeController(IWaitAndRetryService greetingsService, IIdempotentService idempotentService, ITimeoutService timeoutService, ICircuitBreakerService circuitBreakerService, IFallbackService fallbackService)
        {
            _waitAndRetryService = greetingsService;
            _idempotentService = idempotentService;
            _timeoutService = timeoutService;
            _circuitBreakerService = circuitBreakerService;
            _fallbackService = fallbackService;
        }

        [HttpGet("wait-and-retry")]
        public async Task<string> Get() {
            return await _waitAndRetryService.Get();
        }

        [HttpGet("idempotent-wait-and-retry")]
        public async Task<string> IdempotentWaitAndRetry() {
            return await _idempotentService.Get();
        }

        [HttpPost("idempotent-no-op")]
        public async Task IdempotentNoOp() {
            await _idempotentService.Create();
        }

        [HttpGet("timeout")]
        public async Task Timeout() {
            await _timeoutService.Get();
        }

        [HttpGet("circuit-breaker")]
        public async Task CircuitBreaker() {
            await _circuitBreakerService.Get();
        }

        [HttpGet("fallback")]
        public async Task Fallback() {
            await _fallbackService.Get();
        }
    }
}
