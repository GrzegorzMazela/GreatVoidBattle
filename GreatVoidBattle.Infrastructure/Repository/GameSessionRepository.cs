using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.GameState;
using MongoDB.Driver;

namespace GreatVoidBattle.Infrastructure.Repository;

public class GameSessionRepository : IGameSessionRepository
{
    private readonly IMongoCollection<GameSession> _collection;

    public GameSessionRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<GameSession>("GameSessions");
    }

    public async Task<GameSession?> GetActiveSessionAsync()
    {
        return await _collection.Find(s => s.Status == GameSessionStatus.Active)
            .FirstOrDefaultAsync();
    }

    public async Task<GameSession?> GetByIdAsync(string id)
    {
        return await _collection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<GameSession> CreateAsync(GameSession session)
    {
        await _collection.InsertOneAsync(session);
        return session;
    }

    public async Task UpdateAsync(GameSession session)
    {
        session.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(s => s.Id == session.Id, session);
    }

    public async Task<List<GameSession>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task IncrementTurnAsync(string sessionId)
    {
        var session = await GetByIdAsync(sessionId);
        if (session == null) return;

        session.CurrentTurn++;
        session.UpdatedAt = DateTime.UtcNow;
        await UpdateAsync(session);
    }
}