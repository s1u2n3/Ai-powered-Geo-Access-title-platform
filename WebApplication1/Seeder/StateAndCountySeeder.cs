using System.Text.Json;
using WebApplication1.entites;
using WebApplication1.SeedDto_s;

namespace WebApplication1.Seeder
{
    public class StateAndCountySeeder
    {
        public static async Task SeedStatesAndCounties(AppDbContext context)
        {
            var json =
                await File.ReadAllTextAsync(
                    "Data/india-states-counties.json");

            var data =
                JsonSerializer.Deserialize<List<StateSeed>>(json);

            foreach (var item in data)
            {
                // CHECK STATE
                var existingState =
                    context.States
                        .FirstOrDefault(x =>
                            x.StateCode == item.StateCode);

                States state;

                if (existingState == null)
                {
                    state = new States
                    {
                        StateCode = item.StateCode,
                        StateName = item.StateName,
                        CountryCode = item.CountryCode,
                        Latitude = item.Latitude,
                        Longitude = item.Longitude
                    };

                    context.States.Add(state);

                    await context.SaveChangesAsync();
                }
                else
                {
                    state = existingState;
                }

                // COUNTIES
                foreach (var countyItem in item.Counties)
                {
                    var existingCounty =
                        context.Counties
                            .FirstOrDefault(x =>
                                x.CountyCode ==
                                countyItem.CountyCode);

                    if (existingCounty == null)
                    {
                        context.Counties.Add(new County
                        {
                            CountyCode = countyItem.CountyCode,
                            CountyName = countyItem.CountyName,
                            Latitude = countyItem.Latitude,
                            Longitude = countyItem.Longitude,
                            StateId = state.Id
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }

    }
    }

