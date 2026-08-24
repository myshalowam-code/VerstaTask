using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Versta.Orders.Infrastructure.ReadModel;

public sealed class OrderReadDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string SenderCity { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string RecipientCity { get; set; } = string.Empty;
    public string RecipientAddress { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public DateOnly PickupDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid CreatedBy { get; set; }
}
