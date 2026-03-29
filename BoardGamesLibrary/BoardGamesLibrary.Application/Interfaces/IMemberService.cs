using BoardGamesLibrary.Application.Contracts;

namespace BoardGamesLibrary.Application.Interfaces;

public interface IMemberService
{
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken);
    Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<MemberResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<MemberResponse> GetAsync(int id, CancellationToken cancellationToken);
}
