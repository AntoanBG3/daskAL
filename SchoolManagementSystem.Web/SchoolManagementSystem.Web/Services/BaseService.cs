using Microsoft.Extensions.Logging;

namespace SchoolManagementSystem.Web.Services
{
    public abstract class BaseService<T>
    {
        protected readonly ILogger<T> _logger;

        protected BaseService(ILogger<T> logger)
        {
            _logger = logger;
        }

        protected async Task<TResult> ExecuteSafeAsync<TResult>(Func<Task<TResult>> action, string errorMessage, TResult defaultValue = default)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ErrorMessage}", errorMessage);
                return defaultValue;
            }
        }

        protected async Task ExecuteSafeAsync(Func<Task> action, string errorMessage)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ErrorMessage}", errorMessage);
                throw; // Rethrow for void/Task operations where the caller might need to know, or keep consistent with previous behavior?
                       // Previous behavior in Add/Update/Delete was "throw".
                       // Previous behavior in Get was "return null/empty".
            }
        }

        // Overload to allow suppressing throw if needed, though mostly we saw throw in modifications
        protected async Task ExecuteSafeAsync(Func<Task> action, string errorMessage, bool rethrow)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ErrorMessage}", errorMessage);
                if (rethrow) throw;
            }
        }
    }
}
