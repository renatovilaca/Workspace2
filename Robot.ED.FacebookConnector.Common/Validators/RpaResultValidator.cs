using FluentValidation;
using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Common.Validators;

public class RpaResultValidator : AbstractValidator<RpaResultDto>
{
    public RpaResultValidator()
    {
        RuleFor(x => x.TrackId).NotEmpty().WithMessage("TrackId is required");
    }
}
