namespace Fixnow.DTOs.Common;

public class PaginationRequestDto
{
  public int PageIndex { get; set; } = 1;
  public int PageSize { get; set; } = 10;
}
