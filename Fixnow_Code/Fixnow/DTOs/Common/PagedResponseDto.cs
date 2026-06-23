namespace Fixnow.DTOs.Common;

public class PagedResponseDto<T>
{
  public List<T> Items { get; set; } = new();
  public int TotalCount { get; set; }
  public int PageIndex { get; set; }
  public int PageSize { get; set; }
  public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
  public bool HasPreviousPage => PageIndex > 1;
  public bool HasNextPage => PageIndex < TotalPages;
}
