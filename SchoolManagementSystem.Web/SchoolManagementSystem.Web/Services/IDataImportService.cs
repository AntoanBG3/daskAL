namespace SchoolManagementSystem.Web.Services
{
    public interface IDataImportService
    {
        /// <summary>
        /// Reads a legacy JSON file from disk and imports its data.
        /// </summary>
        Task ImportFromLegacyJsonAsync(string jsonPath);

        /// <summary>
        /// Imports school data from a raw JSON string.
        /// </summary>
        Task ImportFromJsonAsync(string jsonContent);
    }
}
