using System.ComponentModel.DataAnnotations;
using Fixnow.Enums;

namespace Fixnow.DTOs.Admin;

public class ReviewWorkerServiceDto
{
  [Required]
  public WorkerServiceStatus Status { get; set; }
}
