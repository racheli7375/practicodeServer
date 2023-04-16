using Microsoft.EntityFrameworkCore;
using TodoAPI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Item>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ToDoDbContext>();
builder.Services.AddCors(option =>
 option.AddPolicy("corsPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
 app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

}
app.MapGet("/", ()=>"working");
   

app.MapGet("/todoitems", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

app.MapGet("/todoitems/complete", async (ToDoDbContext db) =>
    await db.Items.Where(t => t.IsComplete==true).ToListAsync());

app.MapGet("/todoitems/{id}", async (int id, ToDoDbContext db) =>
    await db.Items.FindAsync(id)
        is Item todo
            ? Results.Ok(todo)
            : Results.NotFound());

app.MapPost("/todoitems", async ([FromBody]Item todo, ToDoDbContext db) =>
{
    db.Items.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
});


app.MapPut("/todoitems/{id}", async (int id,bool isComplete, ToDoDbContext db) =>
{
    var todo = await db.Items.FindAsync(id);

    if (todo is null) return Results.NotFound();

   
    todo.IsComplete = isComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/todoitems/{id}", async (int id, ToDoDbContext db) =>
{
    if (await db.Items.FindAsync(id) is Item todo)
    {
        db.Items.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }

    return Results.NotFound();
});

app.UseCors("corsPolicy");
app.Run();
