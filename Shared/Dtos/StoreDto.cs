using System.ComponentModel;
using Newtonsoft.Json;

namespace Shared.Dtos;

public class StoreDto
{
    public required string Id { get; set; }
    public required string StoreKey { get; set; }
    public required string Name { get; set; }
    public required string Address { get; set; }
    public string? StocksDateVerified { get; set; }

    [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan OpeningTime { get; set; }

    [JsonConverter(typeof(TimeSpanConverter))]
    public TimeSpan ClosingTime { get; set; }

    public List<EmployeeDto> Employees { get; set; } = [];
    public List<StoreInventoryDto> StoreInventories { get; set; } = [];

    // The previous delivered stocks
    public List<StoreInventoryDto> PreviousStoreInventories { get; set; } = [];
}
