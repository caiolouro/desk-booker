using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
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
  }
}
