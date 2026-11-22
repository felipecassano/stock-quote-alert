using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StockQuoteAlert.Services;


class PeriodicHostedService : BackgroundService
{
    private readonly ILogger<PeriodicHostedService> _logger;
    private readonly IServiceScopeFactory _factory;
    public PeriodicHostedService(
        ILogger<PeriodicHostedService> logger, IServiceScopeFactory factory)
    {
        _factory = factory;
        _logger = logger;
    }


    private readonly TimeSpan _period = TimeSpan.FromSeconds(5);

    // Se estiver em 0 é pq o preço está no meio, 1 está em alta (venda) e -1 está em baixa (comopra)
    private int _executionCount = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(_period);
        while (
            !stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken)
        ) try
            {
                await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
                StockAlertService sampleService = asyncScope.ServiceProvider.GetRequiredService<StockAlertService>();
                await sampleService.CheckAsync();
                _logger.LogInformation(
                        $"Executed PeriodicHostedService - Count: {_executionCount}");
            }

            catch (Exception ex)
            {
                _logger.LogInformation($"Falha ao executar o PeriodicHostedService. Mensagem: {ex.Message} ");
            }
    }
}