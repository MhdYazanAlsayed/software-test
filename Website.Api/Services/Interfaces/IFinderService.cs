using Website.Models;
using Website.Services.Implementations.DTOs;

namespace Website.Services.Interfaces
{
    /// <summary>
    /// Service responsible for finding months and years where a specific
    /// day of the month falls on a specific day of the week within a given year range.
    /// </summary>
    public interface IFinderService
    {
        /// <summary>
        /// Finds all month-year pairs within the given year range
        /// where the specified day of the month falls on the specified day of the week.
        /// </summary>
        /// <param name="startYear">
        /// The beginning year of the search range (inclusive).
        /// </param>

        /// <param name="endYear">
        /// The ending year of the search range (inclusive).
        /// </param>

        /// <param name="dayOfMonth">
        /// The day within the month (for example: 15).
        /// </param>

        /// <param name="targetDayOfWeek">
        /// The desired day of the week (for example: Saturday).
        /// </param>
        /// <returns>
        /// A list of (Year, Month) pairs where the given day of the month
        /// occurs on the specified day of the week.
        /// </returns>
        ResultDto<List<FindMatchDto>> FindMatches(
            int startYear,
            int endYear,
            int dayOfMonth,
            DayOfWeek targetDayOfWeek);
    }
}
