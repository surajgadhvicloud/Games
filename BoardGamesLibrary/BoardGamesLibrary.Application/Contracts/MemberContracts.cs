using BoardGamesLibrary.Domain.Enums;

namespace BoardGamesLibrary.Application.Contracts;

public sealed record CreateMemberRequest(
    string FirstName,
    string? MiddleName,
    string LastName,
    string Address,
    string PhoneNumber,
    string Email,
    UserType TypeOfUser);

public sealed record UpdateMemberRequest(
    string FirstName,
    string? MiddleName,
    string LastName,
    string Address,
    string PhoneNumber,
    string Email,
    UserType TypeOfUser);

public sealed record MemberResponse(
    int Id,
    string FirstName,
    string? MiddleName,
    string LastName,
    string Address,
    string PhoneNumber,
    string Email,
    UserType TypeOfUser);
