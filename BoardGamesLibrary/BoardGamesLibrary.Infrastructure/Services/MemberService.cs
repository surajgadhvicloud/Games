using BoardGamesLibrary.Application.Contracts;
using BoardGamesLibrary.Application.Interfaces;
using BoardGamesLibrary.Domain.Entities;
using BoardGamesLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BoardGamesLibrary.Infrastructure.Services;

public class MemberService(
    BoardGamesDbContext dbContext,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork) : IMemberService
{
    public async Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken)
    {
        var emailExists = await dbContext.Members.AnyAsync(x => x.Email == request.Email, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Member with this email already exists.");
        }

        var entity = new Member
        {
            FirstName = request.FirstName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            LastName = request.LastName.Trim(),
            Address = request.Address.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            TypeOfUser = request.TypeOfUser,
            CreatedAtUtc = DateTime.UtcNow,
            ModifiedByUser = currentUserService.GetUsername()
        };

        dbContext.Members.Add(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Members.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Member {id} was not found.");

        var emailExists = await dbContext.Members.AnyAsync(x => x.Email == request.Email && x.Id != id, cancellationToken);
        if (emailExists)
        {
            throw new InvalidOperationException("Member with this email already exists.");
        }

        entity.FirstName = request.FirstName.Trim();
        entity.MiddleName = request.MiddleName?.Trim();
        entity.LastName = request.LastName.Trim();
        entity.Address = request.Address.Trim();
        entity.PhoneNumber = request.PhoneNumber.Trim();
        entity.Email = request.Email.Trim().ToLowerInvariant();
        entity.TypeOfUser = request.TypeOfUser;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.ModifiedByUser = currentUserService.GetUsername();

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<PagedResult<MemberResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Members
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToResponse(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<MemberResponse>(items, totalCount, page, pageSize);
    }

    public async Task<MemberResponse> GetAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Members.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Member {id} was not found.");
        return ToResponse(entity);
    }

    private static MemberResponse ToResponse(Member entity) =>
        new(entity.Id, entity.FirstName, entity.MiddleName, entity.LastName, entity.Address, entity.PhoneNumber, entity.Email, entity.TypeOfUser);
}
