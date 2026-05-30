using System;

namespace CellularAutomata5Biomes
{
    class Program
    {
        const int WIDTH = 400;
        const int HEIGHT = 100;
        const int ITERATIONS = 50;

        static Random rng = new Random(2);

        enum CellState
        {
            ocean = 0,
            grass = 1,
            forest = 2,
            dessert = 3,
            mountain = 4,
            beach = 5
        }

        static void Main(string[] args)
        {
            CellState[,] grid = new CellState[WIDTH, HEIGHT];
            CellState[,] nextGrid = new CellState[WIDTH, HEIGHT];

            // inicialización aleatoria
            for (int y = 0; y < HEIGHT; y++)
                for (int x = 0; x < WIDTH; x++)
                    grid[x, y] = (CellState)rng.Next(0, 6);

            Console.CursorVisible = false;

                bool isFinalIteration = false;
            for (int step = 0; step < ITERATIONS; step++)
            {
                if (step == ITERATIONS - 7)
                    isFinalIteration = true;

                Console.SetCursorPosition(0, 0);
                DrawGrid(grid);

                for (int y = 0; y < HEIGHT; y++)
                    for (int x = 0; x < WIDTH; x++)
                        nextGrid[x, y] = ComputeNextState(grid, x, y, isFinalIteration);

                Array.Copy(nextGrid, grid, nextGrid.Length);
                System.Threading.Thread.Sleep(50);
            }
            Console.ResetColor();
            Console.WriteLine("\nSimulación terminada.");
        }

        // ============================================================
        // MAYORÍA
        // ============================================================
        static CellState GetMajorityStateRadius(CellState[,] grid, int x, int y, int radius, CellState current)
        {

            int[] counts = new int[6];

            // cuenta los vecinos dentro del radio
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < WIDTH && ny >= 0 && ny < HEIGHT)
                    {
                        //ignora a beach 
                        if (grid[nx, ny]!= CellState.beach)
                        counts[(int)grid[nx, ny]]++;
                    }
                }
            }

            // calcula la mayoría
            int max = -1;
            int winner = (int)current;
            int ties = 0;

            for (int i = 0; i < counts.Length; i++)
            {
                if (counts[i] > max)
                {
                    max = counts[i];
                    winner = i;
                    ties = 1;
                }
                else if (counts[i] == max)
                {
                    ties++;
                }
            }

            // si hay empate mantiene el estado actual
            if (ties > 1)
                return current;

            return (CellState)winner;
        }

        static int CountNeighborsRadius(CellState[,] grid, int x, int y, int radius, CellState target)
        {
            int count = 0;

            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < WIDTH && ny >= 0 && ny < HEIGHT)
                    {
                        if (grid[nx, ny] == target)
                            count++;
                    }
                }
            }

            return count;
        }

        static CellState ComputeNextState(CellState[,] grid, int x, int y, bool isFinalIteration)
        {
            int[] counts = new int[6];

            // ============================================================
            // CONTAR VECINOS MOORE
            // ============================================================
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;

                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < WIDTH && ny >= 0 && ny < HEIGHT)
                        counts[(int)grid[nx, ny]]++;
                }
            }

            CellState current = grid[x, y];

            // ============================================================
            // REGLA FINAL: ELIMINA CHARCOS DE AGUA
            // ============================================================
            if (isFinalIteration)
            {
                if (current == CellState.ocean &&
                    CountNeighborsRadius(grid, x, y, 3, CellState.ocean) < 40)
                {
                    return GetMajorityStateRadius(grid, x, y, 3, current);
                }
            }

            // ============================================================
            // REGLAS PARA GRASS
            // ============================================================
            if (current == CellState.grass)
            {
                // emerge forest
                if (!isFinalIteration &&
                    counts[(int)CellState.grass] == 8 &&
                    rng.NextDouble() < 0.05)
                    return CellState.forest;

                // estabiliza frontera entre forest y ocean
                if (CountNeighborsRadius(grid, x, y, 2, CellState.forest) >= 4 &&
                    CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 8)
                    return CellState.grass;

                // expande forest
                if (counts[(int)CellState.forest] >= 3 &&
                    CountNeighborsRadius(grid, x, y, 2, CellState.ocean) <= 1 &&
                    rng.NextDouble() < 0.2)
                    return CellState.forest;

                // expande ocean
                if (CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 11 &&
                    rng.NextDouble() < 0.3)
                    return CellState.ocean;

                // emerge dessert
                if (!isFinalIteration &&
                    CountNeighborsRadius(grid, x, y, 2, CellState.ocean) +
                    CountNeighborsRadius(grid, x, y, 2, CellState.forest) == 0 &&
                    rng.NextDouble() < 0.15)
                    return CellState.dessert;

                // expande dessert
                if (counts[(int)CellState.dessert] >= 3 &&
                    rng.NextDouble() < 0.2)
                    return CellState.dessert;
            }

            // ============================================================
            // REGLAS PARA FOREST
            // ============================================================
            if (current == CellState.forest)
            {
                // expande mountain
                if (counts[(int)CellState.mountain] >= 3 &&
                    rng.NextDouble() < 0.4)
                    return CellState.mountain;

                // frontera entre forest y ocean
                if (counts[(int)CellState.ocean] >= 1)
                    return CellState.grass;

                // frontera grande de grass
                if (counts[(int)CellState.grass] >= 2 &&
                    CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 1)
                    return CellState.grass;

                // estabiliza forest
                if (counts[(int)CellState.forest] >= 3)
                    return CellState.forest;
            }

            // ============================================================
            // REGLAS PARA DESSERT
            // ============================================================
            if (current == CellState.dessert)
            {
                // frontera entre dessert y ocean
                if (counts[(int)CellState.ocean] >= 1)
                    return CellState.beach;

                // frontera grande de beach
                if (counts[(int)CellState.beach] >= 2 &&
                    CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 1)
                    return CellState.beach;

                // expande mountain
                if (counts[(int)CellState.mountain] >= 4 &&
                    rng.NextDouble() < 0.25)
                    return CellState.mountain;

                // frontera entre dessert y forest
                if (counts[(int)CellState.forest] >= 1)
                    return CellState.grass;

                // frontera grande de grass
                if (counts[(int)CellState.grass] >= 3 &&
                    CountNeighborsRadius(grid, x, y, 2, CellState.forest) >= 1)
                    return CellState.grass;
            }

            // ============================================================
            // REGLAS PARA MOUNTAIN
            // ============================================================
            if (current == CellState.mountain)
            {
                // expande ocean
                if (counts[(int)CellState.ocean] >= 3 &&
                    rng.NextDouble() < 0.7)
                    return CellState.ocean;

                // expande grass
                if (counts[(int)CellState.grass] >= 4 &&
                    CountNeighborsRadius(grid, x, y, 2, CellState.mountain) <= 9)
                    return CellState.grass;
            }

            // ============================================================
            // REGLAS PARA BEACH
            // ============================================================
            if (current == CellState.beach)
            {
                // expande ocean
                if (CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 10)
                    return CellState.ocean;

                // si no toca ocean no es playa real
                if (CountNeighborsRadius(grid, x, y, 2, CellState.ocean) <= 1)
                    return GetMajorityStateRadius(grid, x, y, 1, current);

                // estabiliza beach
                if (CountNeighborsRadius(grid, x, y, 2, CellState.beach) >= 3)
                    return CellState.beach;
            }

            // ============================================================
            // REGLA DE MAYORÍA
            // ============================================================
            CellState majority = GetMajorityStateRadius(grid, x, y, 1, current);
            if (majority != current)
                return majority;

            // ============================================================
            // SI NADA APLICA SE MANTIENE
            // ============================================================
            return current;
        }


        // ============================================================
        // DIBUJADO
        // ============================================================
        static void DrawGrid(CellState[,] grid)
        {
            for (int y = 0; y < HEIGHT; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    switch (grid[x, y])
                    {
                        case CellState.ocean:
                            Console.ForegroundColor = ConsoleColor.DarkBlue;
                            break;

                        case CellState.grass:
                            Console.ForegroundColor = ConsoleColor.Green;
                            break;

                        case CellState.forest:
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            break;

                        case CellState.dessert:
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            break;

                        case CellState.mountain:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        case CellState.beach:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                    }

                    Console.Write("█");
                }
                Console.WriteLine();
            }
        }
    }
}


