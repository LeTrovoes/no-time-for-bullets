using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject cellPrefab;

    public static readonly int N = 1;
    public static readonly int S = 2;
    public static readonly int E = 4;
    public static readonly int W = 8;

    private readonly Dictionary<int, int> DirectionX = new Dictionary<int, int>
    {{ N, 0 }, { S, 0 }, { E, 1 }, { W, -1 }};

    private readonly Dictionary<int, int> DirectionY = new Dictionary<int, int>
    {{ N, -1 }, { S, 1 }, { E, 0 }, { W, 0 }};

    private readonly Dictionary<int, int> OppositeDirection = new Dictionary<int, int>
    {{ N, S }, { S, N }, { E, W }, { W, E }};

    public int[,] GenerateMaze(int rows, int cols)
    {
        var maze = HuntAndKill(rows, cols);
        InstanciateObjects(maze);
        return maze;
    }

    private int[,] HuntAndKill(int rows, int cols)
    {
        var maze = new int[rows, cols];
        int x = Random.Range(0, cols - 1);
        int y = Random.Range(0, rows - 1);

        while (true)
        {
            (x, y) = Walk(maze, x, y);
            if (x < 0)
            {
                (x, y) = Hunt(maze);
                if (x < 0) break;
            }
        }

        return maze;
    }

    private (int, int) Walk(int[,] maze, int x, int y)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        int nextX, nextY;
        foreach (var direction in Shuffle(new int[] { N, S, E, W }))
        {
            (nextX, nextY) = (x + DirectionX[direction], y + DirectionY[direction]);
            if (nextX >= 0 && nextY >= 0 && nextY < rows && nextX < cols && maze[nextY, nextX] == 0)
            {
                maze[y, x] |= direction;
                maze[nextY, nextX] |= OppositeDirection[direction];

                return (nextX, nextY);
            }
        }
        return (-1, -1);
    }

    private (int, int) Hunt(int [,] maze)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        int nextX, nextY;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (maze[y, x] != 0)
                {
                    continue;
                }

                var neighbors = new List<int>();
                if (y > 0 && maze[y-1, x] != 0) neighbors.Add(N);
                if (x > 0 && maze[y, x-1] != 0) neighbors.Add(W);
                if (x < cols-1 && maze[y, x+1] != 0) neighbors.Add(E);
                if (y < rows-1 && maze[y+1, x] != 0) neighbors.Add(S);

                if (neighbors.Count == 0) continue;
                var direction = neighbors[Random.Range(0, neighbors.Count-1)];
                (nextX, nextY) = (x + DirectionX[direction], y + DirectionY[direction]);
                maze[y, x] |= direction;
                maze[nextY, nextX] |= OppositeDirection[direction];
                return (x, y);
            }
        }
        return (-1, -1);
    }

    private void InstanciateObjects(int[,] maze)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        Vector3 center = new Vector3(cols * 2f, 0, rows * 2f);

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = center;
        plane.transform.localScale = new Vector3(cols * 4f / 10f, 1, rows * 4f / 10f);
        plane.tag = "Ground";
        plane.GetComponent<Renderer>().material.color = new Color(0.45f, 0.478f, 0.514f);

        for (int y = rows - 1; y >= 0; y--)
        {
            for (int x = 0; x < cols; x++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                cell.name = "Cell " + y + ", " + x;
                cell.transform.position = new Vector3(x * 4, 0, (rows - y - 1) * 4);

                int path = maze[y, x];

                if ((path & N) == N)
                {
                    cell.transform.Find("NorthWall").gameObject.SetActive(false);
                    Destroy(cell.transform.Find("NorthWall").gameObject);
                }
                if ((path & S) == S)
                {
                    cell.transform.Find("SouthWall").gameObject.SetActive(false);
                    Destroy(cell.transform.Find("SouthWall").gameObject);
                }
                if ((path & E) == E)
                {
                    cell.transform.Find("EastWall").gameObject.SetActive(false);
                    Destroy(cell.transform.Find("EastWall").gameObject);
                }
                if ((path & W) == W)
                {
                    cell.transform.Find("WestWall").gameObject.SetActive(false);
                    Destroy(cell.transform.Find("WestWall").gameObject);
                }
            }
        }
    }

    public int[] Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = Random.Range(0, array.Length);
            var tmp = array[rnd];
            array[rnd] = array[i];
            array[i] = tmp;
        }
        return array;
    }
}
