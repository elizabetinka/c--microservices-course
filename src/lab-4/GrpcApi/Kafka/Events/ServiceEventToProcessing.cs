namespace GrpcApi.Kafka.Events;

public record ServiceEventToProcessing(long OrderId, DateTime StartedAt) : ServiceEventBase(OrderId);