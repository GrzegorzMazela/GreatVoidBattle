using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.GameState;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Repository for GameSession - handles game session data access.
/// </summary>
public class GameSessionRepository : BaseMongoRepository<GameSession, string>, IGameSessionRepository
{
    public GameSessionRepository(IMongoDatabase database) 
        : base(database, "GameSessions")
    {
    }

    protected override Expression<Func<GameSession, bool>> GetByIdFilter(string id)
    {
        return s => s.Id == id;
    }

    protected override void OnBeforeUpdate(GameSession entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<GameSession?> GetActiveSessionAsync()
    {
        return await FindFirstAsync(s => s.Status == GameSessionStatus.Active);
    }

    public async Task UpdateAsync(GameSession session)
    {
        await UpdateAsync(session, session.Id);
    }

    public async Task IncrementTurnAsync(string sessionId)
    {
        var session = await GetByIdAsync(sessionId);
        if (session == null) return;

        session.CurrentTurn++;
        await UpdateAsync(session);
    }
}
