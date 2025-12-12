using System.Net;
using System.Text.Json;
using Backend.Application.Interfaces;
using Serilog;

namespace Backend.Application.Services;

public class HttpService : IHttpService
    {
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpService(HttpClient httpClient)
        {
        _httpClient = httpClient;

        _jsonOptions = new JsonSerializerOptions
            {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

    public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default) where T : class
        {
        try
            {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                {
                Log.Warning(
                    "HTTP request failed with status code {StatusCode} for URL {Url}",
                    response.StatusCode,
                    url);

                // Handle specific HTTP error codes
                return response.StatusCode switch
                    {
                    HttpStatusCode.NotFound => null,
                    HttpStatusCode.Unauthorized => throw new UnauthorizedAccessException($"Unauthorized access to {url}"),
                    _ => throw new HttpRequestException(
                        $"Request to {url} failed with status code {response.StatusCode}")
                    };
                }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
                {
                Log.Warning("Empty response received from {Url}", url);
                return null;
                }

            var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);

            return result;
            }
        catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
            {
            Log.Warning("Request to {Url} was cancelled", url);
            throw;
            }
        catch (TaskCanceledException ex)
            {
            Log.Error(ex, "Request to {Url} timed out", url);
            throw new TimeoutException($"Request to {url} timed out", ex);
            }
        catch (HttpRequestException ex)
            {
            Log.Error(ex, "HTTP request error for {Url}", url);
            throw;
            }
        catch (JsonException ex)
            {
            Log.Error(ex, "Failed to deserialize response from {Url}", url);
            throw new InvalidOperationException($"Failed to deserialize response from {url}", ex);
            }
        }
    }
