namespace CopticChanter.WebApi.Services;

public class SessionCleaupService(ILogger<SessionCleaupService> _logger, [FromKeyedServices("sessions")] SessionIndex _sessions) : IHostedService, IDisposable
{
    private readonly TimeSpan Interval = TimeSpan.FromMinutes(30);
    private readonly TimeSpan MaxAge = TimeSpan.FromHours(10);
    private Timer? _timer = null;

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Cleanup Service running.");

        _timer = new Timer(DoWork, null, MaxAge.Add(TimeSpan.FromSeconds(1)), Interval);

        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        var now = DateTimeOffset.Now;

        int endedSessions = 0;
        var sessionKeys = _sessions.Keys.ToArray();
        foreach (var key in sessionKeys)
        {
            if (!_sessions.TryGetValue(key, out var session))
                continue;
            
            var lifetime = now - session.LastModified;
            if (lifetime >= MaxAge)
            {
                _sessions.TryRemove(session.Key, out _);
                ++endedSessions;
            }
        }

        _logger.LogInformation("Cleaned up {Count} sessions", endedSessions);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Session Cleanup Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
