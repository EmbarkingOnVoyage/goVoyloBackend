using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Application.Features.Customer.Dtos;
using GoVoylo.Domain.Entities;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.AddGstDetails
{
    public class AddGstDetailsCommandHandler
        : IRequestHandler<AddGstDetailsCommand, GstDetailsDto>
    {
        private readonly ICustomerGstDetailRepository _gstRepository;

        public AddGstDetailsCommandHandler(ICustomerGstDetailRepository gstRepository)
        {
            _gstRepository = gstRepository;
        }

        public async Task<GstDetailsDto> Handle(
            AddGstDetailsCommand request,
            CancellationToken cancellationToken)
        {
            var existing = await _gstRepository.GetByUserIdAsync(request.UserId);

            if (existing != null)
            {
                throw new ConflictException(
                    "gst_details_already_exist",
                    "GST details already exist for this account — use update instead.");
            }

            if (await _gstRepository.GstinExistsForOtherUserAsync(request.Gstin, request.UserId))
            {
                throw new ConflictException("gstin_already_registered", "This GSTIN is already registered.");
            }

            var gst = new CustomerGstDetail(request.UserId, request.Gstin, request.LegalName, request.TradeName);
            await _gstRepository.AddAsync(gst);

            return new GstDetailsDto(gst.Id, gst.Gstin, gst.LegalName, gst.TradeName, gst.IsVerified);
        }
    }
}
