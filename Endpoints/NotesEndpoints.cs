using System.Security.Claims;
using AspNetJWTAuth.DTOS;
using AspNetJWTAuth.Models;

namespace AspNetJWTAuth.Endpoints;

public static class NotesEndpoints
{
    private static readonly List<Note> _notes = [];

    public static void MapNotesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notes")
            .RequireAuthorization()
            .WithTags("Notes");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);
    }

    private static IResult GetAll(ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        return Results.Ok(_notes.Where(n => n.OwnerId == userId));
    }

    private static IResult GetById(int id, ClaimsPrincipal user)
    {
        var note = _notes.FirstOrDefault(n => n.Id == id);
        if (note is null) return Results.NotFound();
        if (note.OwnerId != GetUserId(user)) return Results.Forbid();

        return Results.Ok(note);
    }

    private static IResult Create(NoteRequest request, ClaimsPrincipal user)
    {
        var note = new Note
        {
            Id = _notes.Count == 0 ? 1 : _notes.Max(n => n.Id) + 1,
            Title = request.Title,
            Content = request.Content,
            OwnerId = GetUserId(user)
        };

        _notes.Add(note);
        return Results.Created($"/notes/{note.Id}", note);
    }

    private static IResult Update(int id, NoteRequest request, ClaimsPrincipal user)
    {
        var note = _notes.FirstOrDefault(n => n.Id == id);
        if (note is null) return Results.NotFound();
        if (note.OwnerId != GetUserId(user)) return Results.Forbid();

        note.Title = request.Title;
        note.Content = request.Content;
        return Results.Ok(note);
    }

    private static IResult Delete(int id, ClaimsPrincipal user)
    {
        var note = _notes.FirstOrDefault(n => n.Id == id);
        if (note is null) return Results.NotFound();
        if (note.OwnerId != GetUserId(user)) return Results.Forbid();

        _notes.Remove(note);
        return Results.NoContent();
    }

    private static int GetUserId(ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}