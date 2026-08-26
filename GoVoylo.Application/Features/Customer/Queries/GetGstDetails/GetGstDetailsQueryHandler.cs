using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Queries.GetGstDetails
{
    public class GetGstDetailsQueryHandler
        : IRequestHandler<GetGstDetailsQuery, GstDetailsDto?>
    {
        private readonly ICustomerGstDetailRepository _gstRepository;

        public GetGstDetailsQueryHandler(ICustomerGstDetailRepository gstRepository)
        {
            _gstRepository = gstRepository;
        }

        public async Task<GstDetailsDto?> Handle(
            GetGstDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var gst = await _gstRepository.GetByUserIdAsync(request.UserId);

            return gst == null
                ? null
                : new GstDetailsDto(gst.Id, gst.Gstin, gst.LegalName, gst.TradeName, gst.IsVerified);
        }
    }
}
