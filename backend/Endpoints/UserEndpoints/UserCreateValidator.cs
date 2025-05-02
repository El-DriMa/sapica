using FluentValidation;
using sapica_backend.Data;

namespace sapica_backend.Endpoints.UserEndpoints
{
    public class UserCreateValidator : AbstractValidator<UserCreateRequest>
    {
        public UserCreateValidator(ApplicationDbContext dbContext)
        {
            // FirstName validation
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName is required.")
                .MinimumLength(2).WithMessage("FirstName must be over 2 letters.")
                .MaximumLength(20).WithMessage("FirstName must be under 20 letters.");

            // LastName validation
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName is required.")
                .MinimumLength(2).WithMessage("LastName must be over 2 letters.")
                .MaximumLength(30).WithMessage("LastName must be under 30 letters.");

            // YearBorn validation
            RuleFor(x => x.YearBorn)
                .InclusiveBetween(1930, 2010).WithMessage("Age must be at least 15.");

            // Username validation
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MinimumLength(1).WithMessage("Username must have at least 1 character.")
                .MaximumLength(20).WithMessage("Username cannot be longer than 20 characters.")
                .Matches("^[a-zA-Z0-9_]*$").WithMessage("Username can only contain letters, numbers, and underscores.")
                .Must(username => !dbContext.UserAccount.Any(u => u.Username == username))
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
                .Must(email => !dbContext.UserAccount.Any(u => u.Email == email))
                .WithMessage("Email is already in use.");

            // PhoneNumber validation (Bosnia and Herzegovina format)
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required.")
                .Matches(@"^(\+387|0)[6][0-7][0-9]{6}$").WithMessage("PhoneNumber must be a valid phone number for Bosnia and Herzegovina.");

            // CityId validation
            RuleFor(x => x.CityId)
                .GreaterThan(0).WithMessage("CityId must be greater than 0.")
                .Must(cityId => dbContext.City.Any(c => c.Id == cityId))
                .WithMessage("Provided CityId does not exist in the database.");
        }
    }
}
