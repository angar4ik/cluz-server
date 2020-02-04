namespace CLUZServer.Models
{
    public enum GameState
    {
        Unfilled,
        Filled,
        Locked,
        Finished
    }
    public enum PlayerRole
    {
        None,
        Citizen,
        Mafia,
        Police,
        Ghost,
        Kicked
    }
    public enum PlayerState
    {
        Idle,
        Ready,
    }
}
