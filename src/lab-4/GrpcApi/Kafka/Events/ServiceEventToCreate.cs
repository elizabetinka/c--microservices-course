namespace GrpcApi.Kafka.Events;

public record ServiceEventToCreate(long OrderId, DateTime CreatedAt) : ServiceEventBase(OrderId);