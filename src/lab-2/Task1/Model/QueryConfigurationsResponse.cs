namespace Task1.Model;

public record QueryConfigurationsResponse(IEnumerable<ConfigurationItemDto> Items, string? PageToken);