namespace Domain.Events;

public record EntityChangedEvent(string EntityName, int EntityId, int? GymId = null);
