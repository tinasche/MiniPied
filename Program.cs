using Dapper;
using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var connString = "Data source=./pies.db;";

app.MapGet("/", () =>
{
    using (var conn = new SqliteConnection(connString))
    {
        var sql = "SELECT * FROM Pies";
        var allPies = conn.Query(sql);
        if (allPies != null)
            return Results.Ok(allPies);
    }

    return Results.NotFound();
}).WithName("GetPies");

app.MapGet("/pies/names", () =>
{
    using (var conn = new SqliteConnection(connString))
    {
        var sql = "SELECT Name from Pies";
        var pieNames = conn.Query(sql);

        if (pieNames != null)
            return Results.Ok(pieNames);
    }

    return Results.NoContent();
}).WithName("GetPieNames");

// FIXME: Empty sequence exception from Dapper. Resolve for out of bounds ids 
app.MapGet("/pies/{id}", (int id) =>
{
    using (var conn = new SqliteConnection(connString))
    {
        var sql = $"SELECT * FROM Pies WHERE Id = {id}";
        var result = conn.QuerySingle(sql);
        if (result != null)
            return Results.Ok(result);
        else
            return Results.NotFound();
    }
}).WithName("GetPie");

app.MapPost("/pies", (Pie myPie) =>
{
    if (myPie == null)
        return Results.BadRequest(myPie);

    using (var conn = new SqliteConnection(connString))
    {
        var sql = "INSERT INTO Pies (Name, Calories, IsLight) VALUES (@Name, @Calories, @IsLight)";
        var rowsAffected = conn.Execute(sql, myPie);
        Console.Write($"Rows affected: {rowsAffected}");
    }
    return Results.CreatedAtRoute($"/pies/{myPie.Id}", myPie);
});

app.MapDelete("/pies/{id}", (int id) =>
{
    using (var conn = new SqliteConnection(connString))
    {
        var sql = $"DELETE FROM Pies WHERE Id = {id}";
        var rowsAffected = conn.Execute(sql);
        Console.Write($"Rows affected: {rowsAffected}");
    }

    return Results.Ok();
}).WithName("RemovePie");

app.MapPut("/pies/{id}", (int id, Pie newPie) =>
{
    if (newPie == null)
        return Results.BadRequest();


    using (var conn = new SqliteConnection(connString))
    {
        var sql = $"UPDATE Pies SET Name='{newPie.Name}', Calories={newPie.Calories}, IsLight={newPie.IsLight} WHERE Id = {id}";
        var rowsAffected = conn.Execute(sql);
        Console.Write($"Rows affected: {rowsAffected}");
    }

    return Results.NoContent();
}).WithName("UpdatePie");


app.Run();

record Pie(int Id, string Name, double Calories, bool IsLight);