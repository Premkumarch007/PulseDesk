using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>: IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger ;
        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation("Handling {requestName}", requestName);

            var sw = Stopwatch.StartNew();

            try
            {
                var response = await next();
                sw.Stop();
                _logger.LogInformation("Handled {request} in {Elapse}Ms",requestName, sw.ElapsedMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error handling {request} after {elapse}Ms", requestName, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
