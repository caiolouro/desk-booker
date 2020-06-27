using System;
using System.Linq;
using DeskBooker.Core.DataInterface;
using DeskBooker.Core.Domain;

namespace DeskBooker.Core.Processor
{
    public class DeskBookingRequestProcessor
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

            var availableDesks = _deskRepository.GetAvailableDesks(request.Date);
            if (availableDesks.Count() > 0)
            {
                _deskBookingRepository.Save(Create<DeskBooking>(request));
            }

            return Create<DeskBookingResult>(request);
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