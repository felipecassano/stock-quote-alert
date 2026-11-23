using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StockQuoteAlert.Services;


public class PeriodicHostedService : BackgroundService
{
    private readonly ILogger<PeriodicHostedService> _logger;
    private readonly IServiceScopeFactory _factory;
    private readonly TimeSpan _period;

    public PeriodicHostedService(
        ILogger<PeriodicHostedService> logger, IServiceScopeFactory factory, IConfiguration configuration)
    {
        _factory = factory;
        _logger = logger;
        _period = TimeSpan.FromSeconds(configuration.GetValue<int>("PollingIntervalSeconds", 30));
    }


    // Se estiver em 0 é pq o preço está no meio, 1 está em alta (venda) e -1 está em baixa (comopra)
    private int _stateMachine = 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(_period);

        do
        {
            try
            {
                await using AsyncServiceScope asyncScope = _factory.CreateAsyncScope();
                StockAlertService stockAlertService = asyncScope.ServiceProvider.GetRequiredService<StockAlertService>();
                _stateMachine = await stockAlertService.CheckAsync(_stateMachine);
                _logger.LogInformation(
                        "Executed PeriodicHostedService ");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Falha ao executar o PeriodicHostedService. Mensagem: {ex.Message} ");
            }

        } while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));


    }
}