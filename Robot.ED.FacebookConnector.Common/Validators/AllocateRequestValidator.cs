using FluentValidation;
using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Common.Validators;

public class AllocateRequestValidator : AbstractValidator<AllocateRequestDto>
{
    public AllocateRequestValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty().WithMessage("TrackId is required");
        RuleFor(x => x.Channel).NotEmpty().WithMessage("Channel is required");
    }
}
