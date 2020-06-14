using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Resilience.Service.Controllers 
{
    [ApiController]
    [Route("api/[controller]")]
    public class GreetingsController : ControllerBase 
    {
        private readonly ILogger<GreetingsController> _logger;

        public GreetingsController(ILogger<GreetingsController> logger) 
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            throw new Exception();
        }

        [HttpPost]
        public string Post() 
        {
            throw new Exception();
        }

        [HttpGet("sleepy-greetings")]
        public async Task Sleepy()
        {
            await Task.Delay(15000);
        }
    }
}
