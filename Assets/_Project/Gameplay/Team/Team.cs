using UnityEngine;

public class Team
{
    private const TeamRequest DEFAULT_TEAM = TeamRequest.Blue; 

    private TeamRequest currentTeam;
    private TeamRequest lastTeam;

    public Team()
    {
        currentTeam = DEFAULT_TEAM;
    }

    public int SetTeam(TeamRequest newValue)
    {
        lastTeam = currentTeam;
        currentTeam = newValue;

        return 0;
    }

    public TeamRequest GetTeam() => currentTeam;

    public static TeamRequest SetRandomTeam(int number)
    {        
        // exclusive 3 
        int roll = Random.Range(1, ++number); 

        return roll switch
        {
            1 => TeamRequest.Blue,
            2 => TeamRequest.Red,
            _ => TeamRequest.Spectator
        };
    }
}
