using FluentValidation;
using sapica_backend.Data;

namespace sapica_backend.Endpoints.AdoptionRequestEndpoints
{
    public class AdoptionRequestValidator : AbstractValidator<AdoptionRequestEndpoints.AdoptionRequestCreateRequest>
    {
        public AdoptionRequestValidator(ApplicationDbContext db)
        {
            RuleFor(x => x.AdoptionPostId).NotEmpty().WithMessage("Adoption Post is required.")
                .Must(x => db.AdoptionPost.Any(a => a.Id == x)).WithMessage("You have to choose an existing adoption post.");

            RuleFor(x => x.CityId).NotEmpty().WithMessage("City is required.")
                .Must(x => db.City.Any(c => c.Id == x)).WithMessage("You have to choose an existing city");

            RuleFor(x => x.AnyAnimalsBefore).NotEmpty().WithMessage("Any animals before is required.");

            RuleFor(x => x.FirstName).NotEmpty().WithMessage("First name is required.")
                .MinimumLength(1).WithMessage("Minimum length for name is 1.")
                .MaximumLength(50).WithMessage("Maximum length for name is 50");

            RuleFor(x => x.LastName).NotEmpty().WithMessage("Last name is required.")
           .MinimumLength(1).WithMessage("Minimum length for last name is 1.")
           .MaximumLength(50).WithMessage("Maximum length for last name is 50");

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email needs to be in correct format.");

            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.")
                 .Matches(@"^(\+387|0)[6][0-7][0-9]{6}$").WithMessage("PhoneNumber must be a valid phone number for Bosnia and Herzegovina.");

            RuleFor(x => x.Reason).NotEmpty().WithMessage("Reason is required.")
                .MinimumLength(1).WithMessage("Minimum length for reason is 1.")
                .MaximumLength(255).WithMessage("Maximum length for reason is 255");

            RuleFor(x => x.LivingSpace).NotEmpty().WithMessage("Living space is required.")
                .MinimumLength(1).WithMessage("Minimum length for living space is 1.")
                .MaximumLength(20).WithMessage("Maximum length for living space is 20");

            RuleFor(x => x.Backyard).NotEmpty().WithMessage("Backyard is required.");

            RuleFor(x => x.Age).NotEmpty().WithMessage("Age is required.").
                GreaterThan(17).WithMessage("Age must be greater than 17.")
                .LessThan(100).WithMessage("Age must be less than 100.");

            RuleFor(x => x.FamilyMembers).NotEmpty().WithMessage("Family members is required.").
                InclusiveBetween(0, 30).WithMessage("Family members must be between 0 and 30.");

            RuleFor(x => x.AnyKids).NotEmpty().WithMessage("Any kids is required.");

            RuleFor(x => x.TimeCommitment).NotEmpty().WithMessage("Time commitment is required.").
                InclusiveBetween(0, 24).WithMessage("Time commitment must be between 0 and 24.");

            RuleFor(x => x.PreferredCharacteristic).NotEmpty().WithMessage("Preffered characteristics is required")
                 .MinimumLength(1).WithMessage("PreferredCharacteristic must have at least 1 character.")
                .MaximumLength(255).WithMessage("PrefferedCharacteristics  must be under 255 characters.");



        }
    }
}
