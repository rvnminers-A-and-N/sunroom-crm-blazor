using Microsoft.EntityFrameworkCore;
using SunroomCrm.Blazor.Data;
using SunroomCrm.Shared.DTOs.Auth;
using SunroomCrm.Shared.Enums;
using SunroomCrm.Shared.Interfaces;

namespace SunroomCrm.Blazor.Services.Local;

public class LocalUserService : IUserService
{
    private readonly AppDbContext _db;

    public LocalUserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        return await _db.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role.ToString(),
                AvatarUrl = u.AvatarUrl,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        if (request.Name != null) user.Name = request.Name;
        if (request.Email != null)
        {
            if (await _db.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
                throw new InvalidOperationException("Email already exists.");
            user.Email = request.Email;
        }
        if (request.Password != null)
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
        if (request.AvatarUrl != null) user.AvatarUrl = request.AvatarUrl;
        if (request.Role != null && Enum.TryParse<UserRole>(request.Role, out var role))
            user.Role = role;

        await _db.SaveChangesAsync();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString(),
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException($"User {id} not found.");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
    }
}
