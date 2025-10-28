using System;
using System.Globalization;
using Xunit;

namespace TonysMcpTools.Tests
{
    /// <summary>
    /// Pruebas unitarias para las herramientas de fecha y hora MCP
    /// </summary>
    public class DateTimeToolsTests
    {
        [Fact]
        public void GetCurrentDateTime_ShouldReturnCorrectFormat()
        {
            // Act
            var result = DateTimeTools.GetCurrentDateTime();

            // Assert
            Assert.NotNull(result);
            Assert.Matches(@"^\d{8}_\d{6}$", result); // yyyyMMdd_HHmmss
        }

        [Fact]
        public void GetCurrentDateTime_ShouldBeValidDateTime()
        {
            // Act
            var result = DateTimeTools.GetCurrentDateTime();

            // Assert - Intentar parsear de vuelta a DateTime
            var success = DateTime.TryParseExact(
                result,
                "yyyyMMdd_HHmmss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate);

            Assert.True(success, "El formato de fecha y hora no es válido y no se pudo parsear.");
            // Comprobar que la fecha parseada es reciente (dentro del último minuto)
            Assert.True(parsedDate <= DateTime.Now && parsedDate >= DateTime.Now.AddMinutes(-1));
        }

        [Fact]
        public void GetCurrentDate_ShouldReturnCorrectFormat()
        {
            // Act
            var result = DateTimeTools.GetCurrentDate();

            // Assert
            Assert.NotNull(result);
            Assert.Matches(@"^\d{8}$", result); // yyyyMMdd
        }

        [Fact]
        public void GetCurrentDate_ShouldBeValidDate()
        {
            // Act
            var result = DateTimeTools.GetCurrentDate();

            // Assert - Intentar parsear de vuelta a DateTime
            var success = DateTime.TryParseExact(
                result,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedDate);

            Assert.True(success, "El formato de fecha no es válido y no se pudo parsear.");
            Assert.Equal(DateTime.Today, parsedDate.Date);
        }
    }
}