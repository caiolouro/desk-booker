using System;
using System.Collections.Generic;
using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace DeskBooker.Web.Pages
{
    public class BookDeskModelTests
    {
        private readonly Mock<IDeskBookingRequestProcessor> _processorMock;
        private readonly BookDeskModel _bookDeskModel;
        private readonly DeskBookingResult _deskBookingResult;

        public BookDeskModelTests()
        {
            // Global arrange for all unit tests
            _processorMock = new Mock<IDeskBookingRequestProcessor>();

            _bookDeskModel = new BookDeskModel(_processorMock.Object)
            {
                DeskBookingRequest = new DeskBookingRequest()
            };

            _deskBookingResult = new DeskBookingResult()
            {
                Code = DeskBookingResultCode.Success
            };

            _processorMock
              .Setup(x => x.BookDesk(_bookDeskModel.DeskBookingRequest))
              .Returns(_deskBookingResult);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public void ShouldCallBookDeskMethodFromProcessorIfModelIsValid(int expectedBookDeskCalls, bool isModelValid)
        {
            // Arrange
            if (!isModelValid) _bookDeskModel.ModelState.AddModelError("SomeModelKey", "INVALID_SOME_MODEL_KEY");

            // Act
            _bookDeskModel.OnPost();

            // Assert
            _processorMock.Verify(x => x.BookDesk(_bookDeskModel.DeskBookingRequest), Times.Exactly(expectedBookDeskCalls));
        }

        [Fact]
        public void ShouldAddModelErrorIfNoDeskIsAvailable()
        {
            // Arrange
            _deskBookingResult.Code = DeskBookingResultCode.NoDesksAvailable;

            // Act
            _bookDeskModel.OnPost();

            // Assert
            var modelState = Assert.Contains("DeskBookingRequest.Date", _bookDeskModel.ModelState); // Check there's a model error related to this key
            var modelError = Assert.Single(modelState.Errors); // Check that has only one error
            Assert.Equal("No desk available", modelError.ErrorMessage);
        }

        [Fact]
        public void ShouldNotAddModelErrorIfDeskIsAvailable()
        {
            // Arrange
            // Nothing to be done, because the default result is success by using the constructor defined

            // Act
            _bookDeskModel.OnPost();

            // Assert
            Assert.DoesNotContain("DeskBookingRequest.Date", _bookDeskModel.ModelState); // Check there's no model error related to this key 
        }

        [Theory]
        [InlineData(typeof(PageResult), false, null)]
        [InlineData(typeof(PageResult), true, DeskBookingResultCode.NoDesksAvailable)]
        [InlineData(typeof(RedirectToPageResult), true, DeskBookingResultCode.Success)]
        public void OnPostShouldReturnExpectedActionResult(Type expectedActionResultType, bool isModelValid, DeskBookingResultCode? deskBookingResultCode)
        {
            // Arrange
            if (!isModelValid)
            {
                _bookDeskModel.ModelState.AddModelError("SomeModelKey", "INVALID_SOME_MODEL_KEY");
            }

            if (deskBookingResultCode.HasValue)
            {
                _deskBookingResult.Code = deskBookingResultCode.Value;
            }

            // Act
            IActionResult actionResult = _bookDeskModel.OnPost();

            // Assert
            Assert.IsType(expectedActionResultType, actionResult);
        }

        [Fact]
        public void OnPostShouldRedirectToConfirmationPage()
        {
            // Arrange
            _deskBookingResult.Code = DeskBookingResultCode.Success;
            _deskBookingResult.DeskBookingId = 666; // Iron Maiden number
            _deskBookingResult.FirstName = "Eddie";
            _deskBookingResult.Date = new DateTime(2020, 07, 12);

            // Act
            IActionResult result = _bookDeskModel.OnPost();

            // Assert
            var redirectResult = Assert.IsType<RedirectToPageResult>(result); // Now the result is typed to RedirectToPageResult
            Assert.Equal("BookDeskConfirmation", redirectResult.PageName);

            var resultRouteValues = (IDictionary<string, object>)redirectResult.RouteValues;
            Assert.Equal(3, resultRouteValues.Count);

            var deskBookingId = Assert.Contains("DeskBookingId", resultRouteValues);
            Assert.Equal(_deskBookingResult.DeskBookingId, deskBookingId);

            var firstName = Assert.Contains("FirstName", resultRouteValues);
            Assert.Equal(_deskBookingResult.FirstName, firstName);

            var date = Assert.Contains("Date", resultRouteValues);
            Assert.Equal(_deskBookingResult.Date, date);
        }
    }
}
