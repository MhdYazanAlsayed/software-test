using Website.Services.Implementations;

namespace Website.Tests.UnitTests
{
    public class FinderServiceTests
    {
        private readonly FinderService _service;

        public FinderServiceTests()
        {
            _service = new FinderService();
        }

        [Fact]
        public void FindMatches_ShouldReturnCorrectMonths_WhenInputIsValid()
        {
            // Arrange
            int startYear = 2025;
            int endYear = 2027;
            int dayOfMonth = 15;
            DayOfWeek targetDay = DayOfWeek.Saturday;

            // Act
            var result = _service.FindMatches(startYear, endYear, dayOfMonth, targetDay);

            // Assert
            Assert.True(result.IsSucceeded);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data);
        }

        [Fact]
        public void FindMatches_ShouldFail_WhenStartYearIsGreaterThanEndYear()
        {
            // Arrange
            int startYear = 2030;
            int endYear = 2025;

            // Act
            var result = _service.FindMatches(startYear, endYear, 15, DayOfWeek.Saturday);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void FindMatches_ShouldFail_WhenDayOfMonthIsInvalid()
        {
            // Arrange
            int startYear = 2025;
            int endYear = 2026;
            int dayOfMonth = 40;

            // Act
            var result = _service.FindMatches(startYear, endYear, dayOfMonth, DayOfWeek.Friday);

            // Assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void FindMatches_ShouldReturnEmptyList_WhenNoMatchesExist()
        {
            // Arrange
            int startYear = 2025;
            int endYear = 2025;
            int dayOfMonth = 31;

            // Act
            var result = _service.FindMatches(startYear, endYear, dayOfMonth, DayOfWeek.Monday);

            // Assert
            Assert.True(result.IsSucceeded);
        }

        [Fact]
        public void FindMatches_ShouldSkipInvalidMonthDays()
        {
            // Arrange
            int startYear = 2025;
            int endYear = 2025;
            int dayOfMonth = 31;

            // Act
            var result = _service.FindMatches(startYear, endYear, dayOfMonth, DayOfWeek.Sunday);

            // Assert
            Assert.True(result.IsSucceeded);
        }
    }
}
