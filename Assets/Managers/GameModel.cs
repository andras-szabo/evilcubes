public class GameModel
{
	public int liveEnemyCount;
	public int deadEnemyCount;
	public int bulletsFiredCount;
	public int bulletsHitcount;

	public bool isGameOngoing;
	public GameController.GameResult isGameOver = GameController.GameResult.None;
	public bool hasPlayerWon;
}
