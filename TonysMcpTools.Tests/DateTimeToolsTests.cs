
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit;

namespace TonysMcpTools.Tests
{

    /// <summary>
    /// Pruebas unitarias para las herramientas de fecha y hora MCP
    /// </summary>
    public class DateTimeToolsTests
    {
        #region DateTimeTool_yyyymmdd_hhmmss Tests

        [Fact]
        public void GetCurrentDateTime_ShouldReturnCorrectFormat()
        {
            // Act
            var result = DateTimeTool_yyyymmdd_hhmmss.GetCurrentDateTime();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.Length); // yyyyMMdd_HHmmss = 15 caracteres

            // Verificar formato usando regex
            var formatPattern = @"^\d{8}_\d{6}$"; // 8 dígitos + _ + 6 dígitos
            Assert.Matches(formatPattern, result);
        }

        [Fact]
        public void GetCurrentDateTime_ShouldBeValidDateTime()
        {
            // Act
            var result = DateTimeTool_yyyymmdd_hhmmss.GetCurrentDateTime();

            // Assert - Intentar parsear de vuelta a DateTime
            var success = DateTime.TryParseExact(
                result,
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate);

            Assert.True(success);
            Assert.True(parsedDate <= DateTime.Now);
            Assert.True(parsedDate >= DateTime.Now.AddMinutes(-1)); // Dentro del último minuto
        }

        //[Fact]
        //public void GetCurrentDateTimeUtc_ShouldReturnCorrectFormat()
        //{
        //    // Act
        //    var result = DateTimeTool_yyyymmdd_hhmmss.GetCurrentDateTimeUtc();

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(15, result.Length);

        //    var formatPattern = @"^\d{8}_\d{6}$";
        //    Assert.Matches(formatPattern, result);
        //}

        #endregion

        #region DateTimeTool_yyyyMMdd Tests

        [Fact]
        public void GetCurrentDate_ShouldReturnCorrectFormat()
        {
            // Act
            var result = DateTimeTool_yyyyMMdd.GetCurrentDate();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(8, result.Length); // yyyyMMdd = 8 caracteres

            var formatPattern = @"^\d{8}$"; // 8 dígitos
            Assert.Matches(formatPattern, result);
        }

        [Fact]
        public void GetCurrentDate_ShouldBeValidDate()
        {
            // Act
            var result = DateTimeTool_yyyyMMdd.GetCurrentDate();

            // Assert - Intentar parsear de vuelta a DateTime
            var success = DateTime.TryParseExact(
                result,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate);

            Assert.True(success);
            Assert.Equal(DateTime.Today, parsedDate.Date);
        }

        //[Fact]
        //public void GetCurrentDateUtc_ShouldReturnCorrectFormat()
        //{
        //    // Act
        //    var result = DateTimeTool_yyyyMMdd.GetCurrentDateUtc();

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(8, result.Length);

        //    var formatPattern = @"^\d{8}$";
        //    Assert.Matches(formatPattern, result);
        //}

        #endregion

        #region DateTimeFormatterTool Tests

        //[Fact]
        //public void GetFormattedDateTime_WithDefaultFormat_ShouldReturnCorrectFormat()
        //{
        //    // Act
        //    var result = DateTimeFormatterTool.GetFormattedDateTime();

        //    // Assert
        //    Assert.NotNull(result);

        //    // Verificar que tiene el formato por defecto yyyy-MM-dd HH:mm:ss
        //    var formatPattern = @"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}$";
        //    Assert.Matches(formatPattern, result);
        //}

        //[Fact]
        //public void GetFormattedDateTime_WithCustomFormat_ShouldReturnCorrectFormat()
        //{
        //    // Arrange
        //    var customFormat = "dd/MM/yyyy";

        //    // Act
        //    var result = DateTimeFormatterTool.GetFormattedDateTime(customFormat);

        //    // Assert
        //    Assert.NotNull(result);

        //    var formatPattern = @"^\d{2}/\d{2}/\d{4}$";
        //    Assert.Matches(formatPattern, result);
        //}

        //[Theory]
        //[InlineData("yyyy-MM-dd")]
        //[InlineData("MM/dd/yyyy HH:mm")]
        //[InlineData("dddd, MMMM dd, yyyy")]
        //public void GetFormattedDateTime_WithValidFormats_ShouldReturnValidDates(string format)
        //{
        //    // Act
        //    var result = DateTimeFormatterTool.GetFormattedDateTime(format);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.DoesNotContain("Formato inválido", result);
        //}

        //[Fact]
        //public void GetFormattedDateTime_WithInvalidFormat_ShouldReturnErrorMessage()
        //{
        //    // Arrange
        //    var invalidFormat = "invalid_format_xyz";

        //    // Act
        //    var result = DateTimeFormatterTool.GetFormattedDateTime(invalidFormat);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.StartsWith("Formato inválido:", result);
        //    Assert.Contains(invalidFormat, result);
        //}

        #endregion

        #region Integration Tests

        //[Fact]
        //public void AllDateTimeMethods_ShouldReturnCurrentOrRecentValues()
        //{
        //    // Arrange
        //    var testStart = DateTime.Now;

        //    // Act
        //    var fullDateTime = DateTimeTool_yyyymmdd_hhmmss.GetCurrentDateTime();
        //    var dateOnly = DateTimeTool_yyyyMMdd.GetCurrentDate();
        //    var formatted = DateTimeFormatterTool.GetFormattedDateTime("yyyyMMdd");

        //    var testEnd = DateTime.Now;

        //    // Assert - Todos deberían representar fechas entre testStart y testEnd
        //    var parsedFull = DateTime.ParseExact(fullDateTime, "yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
        //    var parsedDate = DateTime.ParseExact(dateOnly, "yyyyMMdd", CultureInfo.InvariantCulture);
        //    var parsedFormatted = DateTime.ParseExact(formatted, "yyyyMMdd", CultureInfo.InvariantCulture);

        //    Assert.True(parsedFull >= testStart.AddSeconds(-1) && parsedFull <= testEnd.AddSeconds(1));
        //    Assert.Equal(testStart.Date, parsedDate.Date);
        //    Assert.Equal(testStart.Date, parsedFormatted.Date);
        //}

        #endregion
    }
}
