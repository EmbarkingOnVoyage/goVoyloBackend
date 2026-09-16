using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.UpdateGstDetails
{
    public class UpdateGstDetailsCommandHandler
        : IRequestHandler<UpdateGstDetailsCommand, GstDetailsDto>
    {
        private readonly ICustomerGstDetailRepository _gstRepository;

        public UpdateGstDetailsCommandHandler(ICustomerGstDetailRepository gstRepository)
        {
            _gstRepository = gstRepository;
        }

        public async Task<GstDetailsDto> Handle(
            UpdateGstDetailsCommand request,
            CancellationToken cancellationToken)
        {
            var gst = await _gstRepository.GetByUserIdAsync(request.UserId);

            if (gst == null)
            {
                throw new NotFoundException("No GST details found — add them first.");
            }

            if (await _gstRepository.GstinExistsForOtherUserAsync(request.Gstin, request.UserId))
            {
                throw new ConflictException("gstin_already_registered", "This GSTIN is already registered.");
            }

            gst.Update(request.Gstin, request.LegalName, request.TradeName);
            await _gstRepository.UpdateAsync(gst);

            return new GstDetailsDto(gst.Id, gst.Gstin, gst.LegalName, gst.TradeName, gst.IsVerified);
        }
    }
}
