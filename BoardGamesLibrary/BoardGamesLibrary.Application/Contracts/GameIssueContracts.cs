using BoardGamesLibrary.Domain.Enums;

namespace BoardGamesLibrary.Application.Contracts;

public sealed record CreateGameIssueRequest(
    int BoardGameId,
    int UserId,
    DateTime? StartDateUtc,
    DateTime? EndDateUtc,
    GameCondition ConditionGivenOut,
    string? PhotoUrlBeforeIssue = null);

public sealed record UpdateGameIssueRequest(
    DateTime? ReturnDateUtc,
    GameCondition? ConditionGivenIn,
    string? PhotoUrlAfterReturn = null);

public sealed record GameIssueResponse(
    int Id,
    int BoardGameId,
    int UserId,
    string? PhotoUrlBeforeIssue,
    string? PhotoUrlAfterReturn,
    DateTime StartDateUtc,
    DateTime EndDateUtc,
    DateTime? ReturnDateUtc,
    GameCondition ConditionGivenOut,
    GameCondition? ConditionGivenIn,
    decimal OverdueCharges,
    GameIssueStatus Status);