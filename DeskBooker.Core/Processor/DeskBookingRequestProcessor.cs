using System;
using System.Linq;
using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessor : IDeskBookingRequestProcessor
    {
        private readonly IDeskBookingRepository _deskBookingRepository;
        private readonly IDeskRepository _deskRepository;

        public DeskBookingRequestProcessor(IDeskBookingRepository deskBookingRepository, IDeskRepository deskRepository)
        {
            _deskBookingRepository = deskBookingRepository;
            _deskRepository = deskRepository;
        }

        public DeskBookingResult BookDesk(DeskBookingRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var deskBookingResult = Create<DeskBookingResult>(request);

            var availableDesks = _deskRepository.GetAvailableDesks(request.Date);
            if (availableDesks.FirstOrDefault() is Desk availableDesk) // Checks if list is not empty and sets the availableDesk variable
            {
                var deskBooking = Create<DeskBooking>(request);
                deskBooking.DeskId = availableDesk.Id;

                _deskBookingRepository.Save(deskBooking);

                deskBookingResult.DeskBookingId = deskBooking.Id;
                deskBookingResult.Code = DeskBookingResultCode.Success;
            }
            else
            {
                deskBookingResult.Code = DeskBookingResultCode.NoDesksAvailable;
            }

            return deskBookingResult;
        }

        private static T Create<T>(DeskBookingRequest request) where T: DeskBookingBase, new()
        {
            return new T
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Date = request.Date
            };
        }
    }
}