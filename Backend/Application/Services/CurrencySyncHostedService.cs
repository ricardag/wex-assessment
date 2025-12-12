using Backend.Application.Configuration;
using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Backend.Domain.Interfaces;
using Serilog;

namespace Backend.Application.Services;

public class CurrencySyncHostedService : IHostedService
    {
    private readonly IServiceProvider _serviceProvider;
    private readonly string _treasuryApiBaseUrl;
    private Task? _executingTask;

    public CurrencySyncHostedService(IServiceProvider serviceProvider, TreasuryApiSettings treasuryApiSettings)
        {
        _serviceProvider = serviceProvider;
        _treasuryApiBaseUrl = treasuryApiSettings.TreasuryApiBaseUrl;
        }

    public Task StartAsync(CancellationToken cancellationToken)
        {
        Log.Information("Currency sync service is starting");

        _executingTask = Task.Run(async () =>
            {
            // Wait for database to be ready
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            await SyncCurrenciesAsync(cancellationToken);
            }, cancellationToken);

        return Task.CompletedTask;
        }

    public async Task StopAsync(CancellationToken cancellationToken)
        {
        Log.Information("Currency sync service is stopping");

        if (_executingTask != null)
            {
            await _executingTask;
            }
        }

    private async Task SyncCurrenciesAsync(CancellationToken cancellationToken)
        {
        var maxRetries = 5;
        var retryDelay = TimeSpan.FromSeconds(3);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
            {
            try
                {
                using var scope = _serviceProvider.CreateScope();
                var httpService = scope.ServiceProvider.GetRequiredService<IHttpService>();
                var currencyRepository = scope.ServiceProvider.GetRequiredService<ICountryCurrencyRepository>();

                Log.Information("Starting currency data sync from Treasury API (Attempt {Attempt}/{MaxRetries})",
                    attempt, maxRetries);

                var allCurrencies = new List<CountryCurrencyDto>();
                var currentPage = 1;
                var totalPages = 1;

                do
                    {
                    var url =
                        $"{_treasuryApiBaseUrl}?fields=country,currency&page[number]={currentPage}&page[size]=100";
                    var response = await httpService.GetAsync<TreasuryCountriesCurrenciesDto>(url, cancellationToken);

                    if (response == null)
                        {
                        Log.Warning("Empty response from Treasury API on page {Page}", currentPage);
                        break;
                        }

                    allCurrencies.AddRange(response.Data);
                    totalPages = response.Meta.TotalPages;
                    currentPage++;
                    } while (currentPage <= totalPages && !cancellationToken.IsCancellationRequested);

                if (!cancellationToken.IsCancellationRequested)
                    {
                    Log.Information("Fetched {Count} currencies from Treasury API. Updating database...",
                        allCurrencies.Count);

                    await currencyRepository.CreateOrUpdateAllAsync(allCurrencies);

                    Log.Information("Currency data sync completed successfully");
                    return; // Success, exit retry loop
                    }
                }
            catch (Exception ex)
                {
                Log.Error(ex, "Failed to sync currency data from Treasury API (Attempt {Attempt}/{MaxRetries})",
                    attempt, maxRetries);

                if (attempt < maxRetries)
                    {
                    Log.Information("Retrying in {Delay} seconds...", retryDelay.TotalSeconds);
                    await Task.Delay(retryDelay, cancellationToken);
                    }
                else
                    {
                    Log.Error("All retry attempts failed. Currency sync aborted.");
                    }
                }
            }
        }
    }
