using Microsoft.AspNetCore.Mvc;
using sapica_backend.Data.Models;
using sapica_backend.Data;
using Microsoft.EntityFrameworkCore;

namespace sapica_backend.Endpoints.DataSeedEndpoints
{

    [Route("data-seed")]
    [ApiController]
    public class DataSeedGeneratorEndpoint(ApplicationDbContext db) : ControllerBase
    {
        [HttpPost]
        public async Task<string> Generate(CancellationToken cancellationToken = default)
        {

            var countries = new List<Country>
            {
                 new Country {  Name = "Bosnia and Herzegovina", Latitude = 43.9159, Longitude = 17.6791 },
                 new Country { Name = "Croatia", Latitude = 45.1000, Longitude = 15.2000 },
                 new Country { Name = "Serbia", Latitude = 44.0165, Longitude = 21.0059 }
            };

            await db.Country.AddRangeAsync(countries, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            var cities = new List<City>
            {
                new City {  Name = "Sarajevo", PostalCode = "71000", Latitude = 43.8563, Longitude = 18.4131, CountryId = 1 },
                new City {  Name = "Mostar", PostalCode = "88000", Latitude = 43.3438, Longitude = 17.8078, CountryId = 1 },
                new City {  Name = "Banja Luka", PostalCode = "78000", Latitude = 44.7722, Longitude = 17.1910, CountryId = 1 },
                new City {  Name = "Tuzla", PostalCode = "75000", Latitude = 44.5384, Longitude = 18.6766, CountryId = 1 },
                new City {  Name = "Zenica", PostalCode = "72000", Latitude = 44.2017, Longitude = 17.9078, CountryId = 1 },
                new City {  Name = "Bijeljina", PostalCode = "76300", Latitude = 44.7567, Longitude = 19.2164, CountryId = 1 },
                new City { Name = "Prijedor", PostalCode = "79101", Latitude = 44.9819, Longitude = 16.7104, CountryId = 1 },
                new City { Name = "Trebinje", PostalCode = "89101", Latitude = 42.7110, Longitude = 18.3443, CountryId = 1 },
                new City { Name = "Doboj", PostalCode = "74000", Latitude = 44.7330, Longitude = 18.0872, CountryId = 1 },
                new City { Name = "Travnik", PostalCode = "72270", Latitude = 44.2264, Longitude = 17.6657, CountryId = 1 },
                new City { Name = "Čapljina", PostalCode = "88300", Latitude = 43.1214, Longitude = 17.6844, CountryId = 1 },
                new City { Name = "Stolac", PostalCode = "88360", Latitude = 43.0840, Longitude = 17.9573, CountryId = 1 },
                new City { Name = "Gradačac", PostalCode = "76250", Latitude = 44.8781, Longitude = 18.4265, CountryId = 1 },
                new City { Name = "Neum", PostalCode = "88390", Latitude = 42.9264, Longitude = 17.6158, CountryId = 1 },
                new City { Name = "Goražde", PostalCode = "73000", Latitude = 43.6688, Longitude = 18.9758, CountryId = 1 },
                new City { Name = "Cazin", PostalCode = "77220", Latitude = 44.9663, Longitude = 15.9436, CountryId = 1 },
                new City { Name = "Livno", PostalCode = "80101", Latitude = 43.8269, Longitude = 17.0079, CountryId = 1 },
                new City { Name = "Široki Brijeg", PostalCode = "88220", Latitude = 43.3833, Longitude = 17.5930, CountryId = 1 },
                new City { Name = "Zvornik", PostalCode = "75400", Latitude = 44.3869, Longitude = 19.1029, CountryId = 1 },
                new City { Name = "Visoko", PostalCode = "71300", Latitude = 43.9889, Longitude = 18.1784, CountryId = 1 }
             };

            await db.AddRangeAsync(cities, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return "Data generation completed successfully.";

        }

        [HttpGet("countries")]
        public async Task<List<Country>> GetAllCountries(CancellationToken cancellationToken = default)
        {
            return await db.Country.ToListAsync(cancellationToken);
        }

        [HttpGet("cities")]
        public async Task<List<City>> GetAllCities(CancellationToken cancellationToken = default)
        {
            return await db.City.Include(x => x.Country).ToListAsync(cancellationToken);
        }

        [HttpPost("animals")]
        public async Task<string> AddAnimals(CancellationToken cancellationToken = default)
        {
            var animals = new List<Animal>
            {
                new Animal { Name = "Aaron", Gender = "M", Size = "Big", Age = "4 years", Color = "Brown", Weight = 70, AnimalType = "Dog" },
                new Animal { Name = "Rexi", Gender = "M", Size = "Small", Age = "8 years", Color = "Black", Weight = 5, AnimalType = "Dog" },
                new Animal { Name = "Lordi", Gender = "M", Size = "Medium", Age = "10 months", Color = "Black", Weight = 2, AnimalType = "Cat" }
            };

            await db.Animal.AddRangeAsync(animals, cancellationToken);
            await db.SaveChangesAsync();

            return "Data generation completed successfully.";

        }
    }

}
