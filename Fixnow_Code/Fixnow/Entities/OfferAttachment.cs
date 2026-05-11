namespace Fixnow.Entities;

public class OfferAttachment
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public Guid OfferId { get; set; }
  public Guid FileId { get; set; }

  // Navigation
  public WorkerOffer Offer { get; set; } = null!;
  public UploadedFile File { get; set; } = null!;
}
