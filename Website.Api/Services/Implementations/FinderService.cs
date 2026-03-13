using System.Globalization;
using Website.Models;
using Website.Services.Implementations.DTOs;
using Website.Services.Interfaces;

namespace Website.Services.Implementations
{
    public class FinderService : IFinderService
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
        public ResultDto<List<FindMatchDto>> FindMatches(int startYear, int endYear, int dayOfMonth, DayOfWeek targetDayOfWeek)
        {
            var validation = IsInputValid(startYear, endYear, dayOfMonth, targetDayOfWeek);
            if (validation.IsFailure)
            {
                return ResultDto<List<FindMatchDto>>
                        .Failure(new List<FindMatchDto>(), validation.Messages);
            }

            var results = new List<FindMatchDto>();

            for (int year = startYear; year <= endYear; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    if (DateTime.DaysInMonth(year, month) < dayOfMonth)
                        continue;

                    var date = new DateTime(year, month, dayOfMonth);

                    if (date.DayOfWeek == targetDayOfWeek)
                    {
                        results.Add(new FindMatchDto
                        {
                            Year = year,
                            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)
                        });
                    }
                }
            }

            return ResultDto<List<FindMatchDto>>.Success(results);
        }


        /// <summary>
        /// Validates the input parameters provided by the user before executing the date search logic.
        /// Ensures that the year range is valid, the day of the month is within an acceptable range,
        /// and the specified day of the week is defined.
        /// </summary>
        /// <param name="startYear">The starting year of the search range.</param>
        /// <param name="endYear">The ending year of the search range.</param>
        /// <param name="dayOfMonth">The day within the month that should be checked.</param>
        /// <param name="targetDayOfWeek">The desired day of the week to match.</param>
        /// <returns>
        /// A <see cref="ResultDto"/> indicating whether the input is valid,
        /// along with a descriptive message in case of validation failure.
        /// </returns>
        private ResultDto IsInputValid(int startYear, int endYear, int dayOfMonth, DayOfWeek targetDayOfWeek)
        {
            if (startYear > endYear)
                return ResultDto.Failure(new() { "The start year must be less than or equal to the end year." });

            if (dayOfMonth < 1 || dayOfMonth > 31)
                return ResultDto.Failure(new() { "The day of the month must be between 1 and 31." });

            if (!Enum.IsDefined(typeof(DayOfWeek), targetDayOfWeek))
                return ResultDto.Failure(new() { "The specified day of the week is not valid." });

            if (endYear - startYear > 1000)
                return ResultDto.Failure(new() { "The year range is too large. Please select a range of 1000 years or less." });

            return ResultDto.Success(new() { "Input parameters are valid." });
        }
    }
}
