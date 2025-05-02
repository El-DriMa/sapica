using FluentValidation;
using Microsoft.EntityFrameworkCore;
using sapica_backend.Data;

namespace sapica_backend.Endpoints.ShelterEndpoints
{
    public class ShelterEditValidator:AbstractValidator<ShelterEndpoints.ShelterUpdateRequest>
    {
        public ShelterEditValidator(ApplicationDbContext db)
        {
            RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must be under 50 characters.");

            RuleFor(x => x.Owner)
                .NotEmpty().WithMessage("Owner is required.")
                .MaximumLength(50).WithMessage("Owner must be under 50 characters.");

            RuleFor(x => x.YearFounded)
                .InclusiveBetween(1800, 2023).WithMessage("YearFounded must be a valid year.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MinimumLength(1).WithMessage("Address must have at least 1 character.")
                .MaximumLength(100).WithMessage("Address must be under 100 characters.");

            // Username validation
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(1).WithMessage("Username must have at least 1 character.")
                .MaximumLength(20).WithMessage("Username cannot be longer than 20 characters.")
                .Matches("^[a-zA-Z0-9_]*$").WithMessage("Username can only contain letters, numbers, and underscores.")
                .Must(username => !db.UserAccount.Any(u => u.Username == username))
                .WithMessage("Username is already taken.");

            // Password validation
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d\W_]{8,}$")
                .WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one number.");

            // Email validation
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email has to be in a valid format.")
                .Must(email => !db.UserAccount.Any(u => u.Email == email))
                .WithMessage("Email is already in use.");

            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Phone number is required.")
                 .Matches(@"^(\+387|0)[6][0-7][0-9]{6}$").WithMessage("PhoneNumber must be a valid phone number for Bosnia and Herzegovina.");

            RuleFor(x => x.CityId).NotEmpty().WithMessage("City is required.")
                .Must(x => db.City.Any(c => c.Id == x)).WithMessage("You have to choose an existing city");

            RuleFor(x => x.ImageUrl).NotEmpty().WithMessage("Image is required.");

        }
    }
}
