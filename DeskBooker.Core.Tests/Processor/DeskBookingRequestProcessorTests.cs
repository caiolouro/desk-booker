using System;
using DeskBooker.Core.Domain;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequestProcessor _processor;

        // Setup for all file tests
        public DeskBookingRequestProcessorTests()
        {
            _processor = new DeskBookingRequestProcessor();
        }


        [Fact]
        public void BookDeskResponseShouldHaveRequestValues()
        {
            // TDD arrange
            var request = new DeskBookingRequest
            {
                FirstName = "Caio",
                LastName = "Louro",
                Email = "caio@louro.com.br",
                Date = new DateTime(2020, 06, 23)
            };

            // TDD act
            var response = _processor.BookDesk(request);

            // TDD assert
            Assert.NotNull(response);
            Assert.Equal(request.FirstName, response.FirstName);
            Assert.Equal(request.LastName, response.LastName);
            Assert.Equal(request.Email, response.Email);
            Assert.Equal(request.Date, response.Date);
        }

        [Fact]
        public void BookDeskShouldThrowExceptionForNullRequest()
        {
            // Checks if the right type of exception is thrown
            var exception = Assert.Throws<ArgumentNullException>(() => _processor.BookDesk(null));

            // Checks if the exception has the right argument null exposed
            Assert.Equal("request", exception.ParamName);
        }
    }
}
