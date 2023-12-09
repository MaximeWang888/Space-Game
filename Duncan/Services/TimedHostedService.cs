using Shard.Shared.Core;
using Duncan.Services;

public class TimedHostedService : IHostedService, IDisposable
{
    private readonly ITimer? _timer = null;
    private readonly IClock _clock;
    private readonly UnitsService _unitsService;

    public TimedHostedService(IClock clock, UnitsService unitsService)
    {
        _clock = clock;
        _unitsService = unitsService;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _clock.CreateTimer(_ => _unitsService.LaunchAllUnitsFight(), null, TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(6));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}