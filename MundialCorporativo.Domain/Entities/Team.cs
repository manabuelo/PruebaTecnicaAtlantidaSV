using MundialCorporativo.Domain.Common;
using MundialCorporativo.Domain.Events;

namespace MundialCorporativo.Domain.Entities;

public class Team : Entity
{
    private readonly List<Player> _players = new();

    public Team(Guid id, string name)
    {
        Id = id;
        Name = name;
        AddDomainEvent(new TeamCreatedDomainEvent(Guid.NewGuid(), DateTime.UtcNow, id, name));
    }

    private Team()
    {
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public IReadOnlyCollection<Player> Players => _players.AsReadOnly();

    public void Rename(string name)
    {
        Name = name;
    }
}
