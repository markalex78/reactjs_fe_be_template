using System.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Created default CORS policy
// REMINDER: This is not secure for production
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
                      policy =>
                      {
                          policy.WithOrigins("*");
                      });
});

// Add services to the container.
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

// Enable CORS:
app.UseCors();

// Connect to Database Server:
// Update Server for the server url or local server name if being hosted locally
// Update Database for the database name
// Update Integrated Security to true if using Windows Authentication
// Update User ID and Password if using SQL Server Authentication
string connectionString = @"Server=MARKALEXANDER\PEMS_SERVER;Database=PEMS;Integrated Security=true;";
SqlConnection connection = new SqlConnection(connectionString);
connection.Open();
Console.WriteLine("Connected to Database Server");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

// The string "user-types" is the endpoint for the API; Example: http://localhost:7129/user-types
// The lambda function is the code that will be executed when the endpoint is called; A function
// can be made outside of this file then imported back into the file and used as the lambda function
app.MapGet("/user-types", () =>
{
    var userTypes = new List<object>();
    int rowIndex = 0;
    
    // The SQL command to be executed
    SqlCommand command = new SqlCommand("SELECT * FROM [PEMS].[dbo].[UserType]", connection);
    SqlDataReader reader = command.ExecuteReader();

    // Read the data and store it in the userTypes array
    while(reader.Read())
    {
        Console.WriteLine("Type ID: " + reader[0] + " - " + "Type Desc: " + reader[1]);
        userTypes.Add(new { TypeId = reader[0], TypeDesc = reader[1] });
        rowIndex++;
    }

    // Close the reader
    reader.Close();

    Console.WriteLine("User Types: " + userTypes);

    // Return the userTypes array
    // Always remember to return an array or an object.
    // Example for array: return new object[] { "value1", "value2" };
    // Example for object: return new { key1 = "value1", key2 = "value2" };
    return userTypes.ToArray();
});

app.MapFallbackToFile("/index.html");

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
