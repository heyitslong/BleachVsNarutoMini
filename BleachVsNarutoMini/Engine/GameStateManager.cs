namespace BleachVsNarutoMini.Engine
{
    public enum GameState
    {
        Menu,
        Playing,
        GameOver
    }

    public static class GameStateManager
    {
        public static GameState CurrentState { get; set; } = GameState.Playing;
    }
}