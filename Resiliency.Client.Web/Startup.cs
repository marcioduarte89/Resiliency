using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Resiliency.Client.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly.Extensions.Http;
using Polly.Timeout;
using System.Threading;

namespace Resiliency.Client 
{
    public class Startup 
    {
        public Startup(IConfiguration configuration) 
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            WaitAndRetryConfiguration(services);
            IdempotentConfiguration(services);
            TimeoutConfiguration(services);
            CircuitBreakerConfiguration(services);
            FallbackConfiguration(services);
        }

        private static void CircuitBreakerConfiguration(IServiceCollection services)
        {
            services.AddHttpClient<ICircuitBreakerService, CircuitBreakerService>(c =>
                {
                    c.BaseAddress = new Uri("http://localhost:65362");
                })
                .AddTransientHttpErrorPolicy(builder =>
                {
                    return builder.CircuitBreakerAsync(2, TimeSpan.FromSeconds(20), (result, span) => { }, () => { });
                });
        }

        private static void TimeoutConfiguration(IServiceCollection services)
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(1),
                    (result, span) =>
                    {
                        //Do Stuff with error response here
                    });

            var timeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(10, TimeoutStrategy.Pessimistic,
                (context, span, arg3) =>
                {
                    // don't await - otherwise it will wait for the execution 
                    arg3.ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                        }
                        else if (t.IsCanceled)
                        {
                            // If the executed delegates do not honour cancellation, this IsCanceled branch may never be hit.  It can be good practice however to include, in case a Policy configured with TimeoutStrategy.Pessimistic is used to execute a delegate honouring cancellation.
                        }
                        else
                        {
                            // extra logic (if desired) for tasks which complete, despite the caller having 'walked away' earlier due to timeout.
                        }
                    });

                    return arg3;
                });

            services.AddHttpClient<ITimeoutService, TimeoutService>(c => { c.BaseAddress = new Uri("http://localhost:65362"); })
                .AddPolicyHandler(timeoutPolicy)
                .AddPolicyHandler(retryPolicy);
        }

        private static void IdempotentConfiguration(IServiceCollection services)
        {
            services.AddHttpClient<IIdempotentService, IdempotentService>(c =>
                {
                    c.BaseAddress = new Uri("http://localhost:65362");
                })
                .AddPolicyHandler(request =>
                {
                    var policy = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(1),
                            (result, span) =>
                            {
                                //Do Stuff with error response here
                            });

                    return request.Method == HttpMethod.Get ? policy : Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();
                });
        }

        private static void WaitAndRetryConfiguration(IServiceCollection services)
        {
            services.AddHttpClient<IWaitAndRetryService, WaitAndRetryService>(c =>
                {
                    c.BaseAddress = new Uri("http://localhost:65362");
                })
                .AddTransientHttpErrorPolicy(builder =>
                {
                    return builder.WaitAndRetryAsync(3, x => TimeSpan.FromSeconds(1),
                        (result, span) =>
                        {
                            //Do Stuff with error response here
                        });
                });
        }

        private void FallbackConfiguration(IServiceCollection services)
        {
            var fallbackPolicy =
                Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .FallbackAsync(FallbackAction, OnFallbackAsync);

            var retryPolicy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(1),
                    (result, span, retryCount, ctx) =>
                    {
                        Console.WriteLine($"Retrying({retryCount})...");
                    });

            var wrapOfRetryAndFallback = Policy.WrapAsync(fallbackPolicy, retryPolicy);

            services.AddHttpClient<IFallbackService, FallbackService>(c =>
                {
                    c.BaseAddress = new Uri("http://localhost:65362");
                }).AddPolicyHandler(wrapOfRetryAndFallback);
        }

        private Task OnFallbackAsync(DelegateResult<HttpResponseMessage> response, Context context) 
        {
            Console.WriteLine("About to call the fallback action. This is a good place to do some logging");
            return Task.CompletedTask;
        }

        private Task<HttpResponseMessage> FallbackAction(DelegateResult<HttpResponseMessage> responseToFailedRequest, 
            Context context, CancellationToken cancellationToken) 
        {
            Console.WriteLine("Fallback action is executing");

            var httpResponseMessage = new HttpResponseMessage(responseToFailedRequest.Result.StatusCode) 
            {
                Content = new StringContent($"The fallback executed, the original error was {responseToFailedRequest.Result.ReasonPhrase}")
            };
            return Task.FromResult(httpResponseMessage);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) 
        {
            if (env.IsDevelopment()) 
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
