namespace Backend.Application.Interfaces;

public interface IHttpService
    {
    Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default) where T : class;
    }
