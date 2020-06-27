using System;
using System.Collections.Generic;
using System.Linq;
using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;
using Moq;
using Xunit;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessorTests
    {
        private readonly DeskBookingRequest _request;
        private readonly List<Desk> _availableDesks;
        private readonly Mock<IDeskBookingRepository> _deskBookingRepositoryMock;
        private readonly Mock<IDeskRepository> _deskRepositoryMock;
        private readonly DeskBookingRequestProcessor _processor;

        // Setup for all tests inside this file
        public DeskBookingRequestProcessorTests()
        {
            _request = new DeskBookingRequest
            {
                FirstName = "Caio",
                LastName = "Louro",
                Email = "caio@louro.com.br",
                Date = new DateTime(2020, 06, 23)
            };

            _availableDesks = new List<Desk> { new Desk() { Id = 1 } };

            _deskBookingRepositoryMock = new Mock<IDeskBookingRepository>();

            _deskRepositoryMock = new Mock<IDeskRepository>();
            // Setup to return one available desk for the request date
            _deskRepositoryMock
                .Setup(repository => repository.GetAvailableDesks(_request.Date))
                .Returns(_availableDesks);

            _processor = new DeskBookingRequestProcessor(_deskBookingRepositoryMock.Object, _deskRepositoryMock.Object);
        }


        [Fact]
        public void BookDeskResponseShouldHaveRequestValues()
        {
            // TDD act
            var response = _processor.BookDesk(_request);

            // TDD assert
            Assert.NotNull(response);
            Assert.Equal(_request.FirstName, response.FirstName);
            Assert.Equal(_request.LastName, response.LastName);
            Assert.Equal(_request.Email, response.Email);
            Assert.Equal(_request.Date, response.Date);
        }

        [Fact]
        public void BookDeskShouldThrowExceptionForNullRequest()
        {
            // Checks if the right type of exception is thrown and returns it
            var exception = Assert.Throws<ArgumentNullException>(() => _processor.BookDesk(null));

            // Checks if the exception has the right argument null exposed
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public void ShouldSaveDeskBooking()
        {
            DeskBooking deskBooking = null;

            // Setup the Save method behaviour
            _deskBookingRepositoryMock
                .Setup(repository => repository.Save(It.IsAny<DeskBooking>())) // When Save method is called with a DeskBooking object parameter
                .Callback<DeskBooking>(savedDeskBooking => deskBooking = savedDeskBooking);

            _processor.BookDesk(_request); // This processor instance was injected with the mocked repository instance that was setup above

            _deskBookingRepositoryMock.Verify(repository => repository.Save(It.IsAny<DeskBooking>()), Times.Once); // Check method was called only once

            Assert.NotNull(deskBooking);
            Assert.Equal(_request.FirstName, deskBooking.FirstName);
            Assert.Equal(_request.LastName, deskBooking.LastName);
            Assert.Equal(_request.Email, deskBooking.Email);
            Assert.Equal(_request.Date, deskBooking.Date);
            Assert.Equal(_availableDesks.First().Id, deskBooking.DeskId);
        }

        [Fact]
        public void ShouldNotSaveDeskBookingIfNoDeskIsAvailable()
        {
            // Force that no desks are available
            _availableDesks.Clear();

            _processor.BookDesk(_request);

            _deskBookingRepositoryMock.Verify(repository => repository.Save(It.IsAny<DeskBooking>()), Times.Never);
        }

        // To use data driven tests, that has different behaviours depending on the input, use the Theory annotation insted of the Fact
        [Theory]
        [InlineData(DeskBookingResultCode.Success, true)]
        [InlineData(DeskBookingResultCode.NoDesksAvailable, false)]
        public void ShouldReturnCorrectResultCode(DeskBookingResultCode correctDeskBookingResultCode, bool isDesksAvailable)
        {
            if (!isDesksAvailable) _availableDesks.Clear();

            var deskBookingResult = _processor.BookDesk(_request);
            Assert.Equal(correctDeskBookingResultCode, deskBookingResult.Code);
        }

        [Theory]
        [InlineData(2, true)]
        [InlineData(null, false)]
        public void ShouldReturnCorrectDeskBookingId(int? correctDeskBookingId, bool isDesksAvailable)
        {
            if (!isDesksAvailable)
            {
                _availableDesks.Clear();
            }
            else
            {
                _deskBookingRepositoryMock
                    .Setup(repository => repository.Save(It.IsAny<DeskBooking>()))
                    .Callback<DeskBooking>(savedDeskBooking => savedDeskBooking.Id = correctDeskBookingId);
            }

            var deskBookingResult = _processor.BookDesk(_request);
            Assert.Equal(correctDeskBookingId, deskBookingResult.DeskBookingId);
        }
    }
}
