using System.Text.Json.Serialization;

namespace Task3.Models.Payloads;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CreatedOrder), typeDiscriminator: nameof(CreatedOrder))]
[JsonDerivedType(typeof(ItemAdded), typeDiscriminator: nameof(ItemAdded))]
[JsonDerivedType(typeof(ItemRemoved), typeDiscriminator: nameof(ItemRemoved))]
[JsonDerivedType(typeof(StateChanged), typeDiscriminator: nameof(StateChanged))]
[JsonDerivedType(typeof(ProcessingStateChanged), typeDiscriminator: nameof(ProcessingStateChanged))]
public record PayloadBaseModel(long OrderId);