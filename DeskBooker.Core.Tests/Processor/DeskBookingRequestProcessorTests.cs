using System;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        [Fact]
        public void ShouldReturnDeskBookingResponseWithRequestValues()
        {
            // TDD arrange
            var request = new DeskBookingRequest
            {
                FirstName = "Caio",
                LastName = "Louro",
                Email = "caio@louro.com.br",
                Date = new DateTime(2020, 06, 23)
            };
            var processor = new DeskBookingRequestProcessor();

            // TDD act
            var result = processor.BookDesk(request);

            // TDD assert
            Assert.NotNull(result);
            Assert.Equal(request.FirstName, result.FirstName);
            Assert.Equal(request.LastName, result.LastName);
            Assert.Equal(request.Email, result.Email);
            Assert.Equal(request.Date, result.Date);
        }
    }
}
