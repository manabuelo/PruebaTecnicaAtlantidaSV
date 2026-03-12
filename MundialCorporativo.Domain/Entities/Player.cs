namespace MundialCorporativo.Domain.Entities;

public class Player
{
    public Player(Guid id, Guid teamId, string fullName, int jerseyNumber)
    {
        Id = id;
        TeamId = teamId;
        FullName = fullName;
        JerseyNumber = jerseyNumber;
    }

    private Player()
    {
    }

    public Guid Id { get; private set; }
    public Guid TeamId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public int JerseyNumber { get; private set; }
    public int GoalsScored { get; private set; }

    public void Update(string fullName, int jerseyNumber)
    {
        FullName = fullName;
        JerseyNumber = jerseyNumber;
    }

    public void AddGoals(int goals)
    {
        GoalsScored += goals;
    }
}
