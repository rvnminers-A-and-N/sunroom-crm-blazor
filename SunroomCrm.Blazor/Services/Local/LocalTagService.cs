using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.Tags;
using SunroomCrm.Shared.Interfaces;
using SunroomCrm.Shared.Models;

namespace SunroomCrm.Blazor.Services.Local;

public class LocalTagService : ITagService
{
    private readonly AppDbContext _db;

    public LocalTagService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TagDto>> GetAllAsync()
    {
        return await _db.Tags
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<TagDto?> GetByIdAsync(int id)
    {
        var tag = await _db.Tags.FindAsync(id);
        if (tag == null) return null;

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            CreatedAt = tag.CreatedAt
        };
    }

    public async Task<TagDto> CreateAsync(CreateTagRequest request)
    {
        if (await _db.Tags.AnyAsync(t => t.Name == request.Name))
            throw new InvalidOperationException($"Tag '{request.Name}' already exists.");

        var tag = new Tag
        {
            Name = request.Name,
            Color = request.Color
        };

        _db.Tags.Add(tag);
        await _db.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            CreatedAt = tag.CreatedAt
        };
    }

    public async Task<TagDto> UpdateAsync(int id, UpdateTagRequest request)
    {
        var tag = await _db.Tags.FindAsync(id)
            ?? throw new KeyNotFoundException($"Tag {id} not found.");

        if (await _db.Tags.AnyAsync(t => t.Name == request.Name && t.Id != id))
            throw new InvalidOperationException($"Tag '{request.Name}' already exists.");

        tag.Name = request.Name;
        tag.Color = request.Color;

        await _db.SaveChangesAsync();

        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            CreatedAt = tag.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await _db.Tags.FindAsync(id)
            ?? throw new KeyNotFoundException($"Tag {id} not found.");

        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync();
    }
}
