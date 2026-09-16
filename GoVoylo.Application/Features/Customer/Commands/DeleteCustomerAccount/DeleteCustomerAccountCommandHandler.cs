using GoVoylo.Application.Common.Exceptions;
using GoVoylo.Domain.Interfaces;
using MediatR;

namespace GoVoylo.Application.Features.Customer.Commands.DeleteCustomerAccount
{
    // Ticket GV-CUST-BE-008 also requires blocking deletion when active bookings exist —
    // the Bookings domain doesn't exist in this codebase yet, so that check is still a no-op.
    public class DeleteCustomerAccountCommandHandler
        : IRequestHandler<DeleteCustomerAccountCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public DeleteCustomerAccountCommandHandler(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<Unit> Handle(
            DeleteCustomerAccountCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new NotFoundException("Customer profile not found.");
            }

            user.MarkDeleted();
            await _userRepository.UpdateAsync(user);
            await _refreshTokenRepository.RevokeAllForUserAsync(request.UserId);

            return Unit.Value;
        }
    }
}
