namespace Backend.Application.Dtos;

public class PagedResultDto<T>
    {
    public IEnumerable<T> Items { get; init; } = new List<T>();
    public int Count { get; init; }
    }
