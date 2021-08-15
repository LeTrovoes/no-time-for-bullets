using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard,
    }

    [Header("Game Settings")]
    [SerializeField] private Difficulty difficulty;

    [HideInInspector] public int initialPickups;
    [HideInInspector] public int initialCountdown;
    [HideInInspector] public int initialEnemyLife;
    private int mazeSize;
    private int numberOfEnemies;

    public int score;
    public float life;
    public float countdown;

    public bool running = false;
    public bool gameOver = false;

    [HideInInspector] public int gunDamage;
    [HideInInspector] public int gunCapacity;
    [HideInInspector] public bool gunAutomatic;
    [HideInInspector] public float gunFirePeriod;
    [HideInInspector] public int remainingShots;
    [HideInInspector] public int remainingAmmo;

    [SerializeField] private GameObject pickUpPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject healPrefab;

    private readonly Dictionary<Difficulty, (int, int, int, int, int)> difficultyToSettings = new Dictionary<Difficulty, (int, int, int, int, int)>
    {
        // pickUps, countdown, enemyLife, size, enemies
        { Difficulty.Easy,   (10, 150, 15,  8, 10) },
        { Difficulty.Medium, (20, 200, 20, 10, 15) },
        { Difficulty.Hard,   (30, 250, 30, 12, 20) },
    };

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this);

        Screen.fullScreen = true;
    }

    private void FixedUpdate()
    {
        if (running)
        {
            countdown = Mathf.Clamp(countdown - Time.fixedDeltaTime, 0, Mathf.Infinity);
            if (countdown == 0)
            {
                EndGame();
            }
        }
    }

    public void StartNewGame(Difficulty _difficulty)
    {
        difficulty = _difficulty;
        (initialPickups, initialCountdown, initialEnemyLife, mazeSize, numberOfEnemies) = difficultyToSettings[difficulty];
        countdown = initialCountdown;
        life = 100;
        score = 0;
        gameOver = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void CreateGameWorld()
    {
        GameObject maze = GameObject.FindGameObjectWithTag("Maze");
        var mazeMap = maze.GetComponent<MazeGenerator>().GenerateMaze(mazeSize, mazeSize);

        GameObject navMeshObj = GameObject.FindGameObjectWithTag("NavMesh");
        navMeshObj.GetComponent<NavMeshSurface>().BuildNavMesh();
        var pickUpsMap = PositionPickUps(initialPickups, mazeMap);
        PositionEnemies(numberOfEnemies, mazeMap);
        PositionHealingItens(3, mazeMap, pickUpsMap);
        running = true;
        Time.timeScale = 1f;
    }

    public void Score()
    {
        score++;
        if (score == initialPickups)
        {
            EndGame();
        }
    }

    public void GetShot(int damage)
    {
        SceneManager.Instance.GetShot();
        life -= damage;
        life = Mathf.Clamp(life, 0, Mathf.Infinity);

        if (life <= 0)
        {
            EndGame();
        }
    }

    public void TogglePause()
    {
        if (gameOver) return;
        if (running)
        {
            running = false;
            Time.timeScale = 0f;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            running = true;
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void EndGame()
    {
        running = false;
        gameOver = true;
        Time.timeScale = 0f;
        SceneManager.Instance.GameEnded();
        Camera.main.GetComponent<AudioSource>().Stop();
    }

    private int[,] PositionPickUps(int numberOfPickups, int[,] mazeMap)
    {
        int rows = mazeMap.GetLength(0);
        int cols = mazeMap.GetLength(1);
        int dist = 2;
        var pickUpsMap = new int[rows, cols];
        GameObject pickUpsObj = new GameObject("Pick Ups");

        int count = 0;

        // cells with a single path
        int[] deadEnds = new int[] { 1, 2, 4, 8 };

        /*
         * 1 4 2 8
         * N E S W
         * 1 4 _ _
         * 1 _ _ 8
         * _ 4 2 _
         * _ _ 2 8
         */
        int[] corners = new int[] { 5, 6, 9, 10 };

        var cornerPositions = new List<(int, int)>();

        pickUpsMap[rows - 1, 0] = 1;
        InvalidatePositions(pickUpsMap, mazeMap, (rows-1 , 0), dist);

        // place pick ups in corners (three walls)
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (deadEnds.Contains(mazeMap[y, x]))
                {
                    if (pickUpsMap[y, x] != 0) continue;
                    InstancePickUp(pickUpsMap, x, y, count, pickUpsObj.transform);
                    InvalidatePositions(pickUpsMap, mazeMap, (y, x), dist);
                    count++;
                    if (count >= numberOfPickups)
                        return pickUpsMap;
                }
                else if (corners.Contains(mazeMap[y, x]))
                {
                    cornerPositions.Add((y, x));
                }
            }
        }

        ShuffleList(cornerPositions);

        for (int i = 0; numberOfPickups > count && i < cornerPositions.Count; i++)
        {
            var (y, x) = cornerPositions[i];

            if (pickUpsMap[y, x] != 0) continue;
            InstancePickUp(pickUpsMap, x, y, count, pickUpsObj.transform);
            InvalidatePositions(pickUpsMap, mazeMap, (y, x), dist);
            count++;
        }

        int attempts = 0;

        for (int i = count; i < numberOfPickups; i++)
        {
            int x;
            int y;

            do
            {
                x = Random.Range(0, cols - 1);
                y = Random.Range(0, rows - 1);
                attempts++;
            } while (pickUpsMap[y, x] != 0 && attempts < 10000);

            if (attempts == 10000)
            {
                attempts = 0;
                do
                {
                    x = Random.Range(0, cols - 1);
                    y = Random.Range(0, rows - 1);
                    attempts++;
                } while ((pickUpsMap[y, x] != 0 || pickUpsMap[y, x] != -2) && attempts < 10000);
            }

            InstancePickUp(pickUpsMap, x, y, i, pickUpsObj.transform);
            InvalidatePositions(pickUpsMap, mazeMap, (y, x), dist);
        }

        return pickUpsMap;
    }

    private void InstancePickUp(int[,] pickUpsMap, int x, int y, int n, Transform parent)
    {
        pickUpsMap[y, x] = 2;
        Vector3 position = new Vector3(x * 4 + Random.Range(0.3f, 3.7f), pickUpPrefab.transform.position.y, (pickUpsMap.GetLength(0) - y - 1) * 4 + Random.Range(0.3f, 3.7f));
        GameObject newPickUp = Instantiate(pickUpPrefab, parent);
        newPickUp.transform.position = position;
        newPickUp.name = "PickUp " + n;
    }

    private void PositionEnemies(int numberOfEnemies, int[,] mazeMap)
    {
        int rows = mazeMap.GetLength(0);
        int cols = mazeMap.GetLength(1);
        var enemiesMap = new int[rows, cols];
        enemiesMap[rows - 1, 0] = 1;
        InvalidatePositionPlayer(enemiesMap, mazeMap, (rows - 1, 0));

        GameObject enemiesObj = new GameObject("Enemies");

        int attempts = 0;

        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 position;
            int x;
            int y;

            do
            {
                x = Random.Range(0, cols - 1);
                y = Random.Range(0, rows - 1);
                attempts++;
            } while (enemiesMap[y, x] != 0 && attempts < 10000);

            if (attempts == 10000)
            {
                attempts = 0;
                do
                {
                    x = Random.Range(0, cols - 1);
                    y = Random.Range(0, rows - 1);
                    attempts++;
                } while ((enemiesMap[y, x] != 0 || enemiesMap[y, x] != -2) && attempts < 10000);
            }

            position = new Vector3(x * 4 + 2, 0, (rows - y - 1) * 4 + 2);

            enemiesMap[y, x] = 2;
            InvalidatePositions(enemiesMap, mazeMap, (y, x), 3);
            attempts = 0;

            GameObject newEnemy = Instantiate(enemyPrefab, enemiesObj.transform);
            newEnemy.transform.position = position;
            newEnemy.name = "Enemy " + i;
        }
    }

    private void PositionHealingItens(int numberOfHealingItens, int[,] mazeMap, int[,] pickUpsMap)
    {
        int rows = mazeMap.GetLength(0);
        int cols = mazeMap.GetLength(1);
        var healingItemMap = pickUpsMap;

        GameObject healingItensObj = new GameObject("Healing Items");
        int attempts = 0;

        for (int i = 0; i < numberOfHealingItens; i++)
        {
            Vector3 position;
            int x;
            int y;

            do
            {
                x = Random.Range(0, cols - 1);
                y = Random.Range(0, rows - 1);
                attempts++;
            } while (healingItemMap[y, x] != 0 && attempts < 10000);

            if (attempts == 10000)
            {
                attempts = 0;
                do
                {
                    x = Random.Range(0, cols - 1);
                    y = Random.Range(0, rows - 1);
                    attempts++;
                } while (healingItemMap[y, x] > 0 && attempts < 10000);
            }

            position = new Vector3(x * 4 + Random.Range(0.3f, 3.7f), healPrefab.transform.position.y, (rows - y - 1) * 4 + Random.Range(0.3f, 3.7f));

            healingItemMap[y, x] = 2;
            InvalidatePositions(healingItemMap, mazeMap, (y, x), 3);
            attempts = 0;

            GameObject newHeal = Instantiate(healPrefab, healingItensObj.transform);
            newHeal.transform.position = position;
            newHeal.name = "Healing Item " + i;
        }
    }


    private readonly Dictionary<(int, int), int> Direction = new Dictionary<(int, int), int>
    {{ (-1,0), MazeGenerator.N }, { (1,0), MazeGenerator.S }, { (0,1), MazeGenerator.E }, { (0,-1), MazeGenerator.W }};

    private void InvalidatePositions(int[,] map, int[,] maze, (int, int) position, int dist)
    {
        if (dist < 0)
            return;
        if (map[position.Item1, position.Item2] == 0)
            map[position.Item1, position.Item2] = -2;
        foreach (var dir in Direction.Keys)
        {
            if (!IsWall(maze[position.Item1, position.Item2], dir))
                InvalidatePositions(map, maze, (position.Item1 + dir.Item1, position.Item2 + dir.Item2), dist - 1);
        }
    }

    private void InvalidatePositionPlayer(int[,] map, int[,] maze, (int, int) position)
    {
        for (int y = position.Item1; y > 0; y--)
        {
            if (map[y, position.Item2] == 0)
                map[y, position.Item2] = -1;
            if (IsWall(maze[y, position.Item2], (-1, 0)))
                break;
        }
        for (int y = position.Item1; y < maze.GetLength(0); y++)
        {
            if (map[y, position.Item2] == 0)
                map[y, position.Item2] = -1;
            if (IsWall(maze[y, position.Item2], (1, 0)))
                break;
        }
        for (int x = position.Item2; x > 0; x--)
        {
            if (map[position.Item1, x] == 0)
                map[position.Item1, x] = -1;
            if (IsWall(maze[position.Item1, x], (0, -1)))
                break;
        }
        for (int x = position.Item2; x < maze.GetLength(1); x++)
        {
            if (map[position.Item1, x] == 0)
                map[position.Item1, x] = -1;
            if (IsWall(maze[position.Item1, x], (0, 1)))
                break;
        }
        InvalidatePositions(map, maze, position, 2);
    }

    private bool IsWall(int cell, (int, int) dir)
    {
        return (cell & Direction[dir]) == 0;
    }

    private List<T> ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(0, list.Count);
            var tmp = list[rnd];
            list[rnd] = list[i];
            list[i] = tmp;
        }
        return list;
    }
}
