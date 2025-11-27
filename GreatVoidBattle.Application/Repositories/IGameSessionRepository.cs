using GreatVoidBattle.Core.Domains.GameState;

namespace GreatVoidBattle.Application.Repositories;

public interface IGameSessionRepository
{
    Task<GameSession?> GetActiveSessionAsync();

    Task<GameSession?> GetByIdAsync(string id);

    Task<GameSession> CreateAsync(GameSession session);

    Task UpdateAsync(GameSession session);

    Task<List<GameSession>> GetAllAsync();

    Task IncrementTurnAsync(string sessionId);
}