namespace Fixnow.Services.Interfaces;

public interface ISystemJobService
{
  Task CleanupExpiredDataAsync();
}
