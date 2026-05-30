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

            // Inicialización aleatoria
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

            // Contar vecinos dentro del radio
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

            // Calcular mayoría
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

            // Si hay empate → mantener estado actual
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

        // ============================================================
        // REGLAS ECOLÓGICAS + MAYORÍA
        // ============================================================
        static CellState ComputeNextState(CellState[,] grid, int x, int y, bool isFinalIteration)
        {
            int[] counts = new int[6];

            // Contar vecinos Moore
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

            if (isFinalIteration)
            {
                // elimina los charcos de agua
                if (current == CellState.ocean && CountNeighborsRadius(grid, x, y, 3, CellState.ocean) < 40)
                {
                    return GetMajorityStateRadius(grid, x, y, 3, current);
                }

            }

            // ============================================================
            // REGLAS ECOLÓGICAS
            // ============================================================


            if (!isFinalIteration)
            {
                // emerge forest
                if (current == CellState.grass && counts[(int)CellState.grass] == 8)
                {
                    if (rng.NextDouble() < 0.05)
                        return CellState.forest;
                }
            }

            // expande mountain
            if (current == CellState.forest && counts[(int)CellState.mountain] >= 3)
            {
                if (rng.NextDouble() < 0.4)
                    return CellState.mountain;
            }

            // forest no puede tocar agua, frontera de grass entre forest y ocean
            if (current == CellState.forest && counts[(int)CellState.ocean] >= 1)
            {
                return CellState.grass;
            }

            // estabiliza frontera entre forest y ocean
            if (current == CellState.grass && CountNeighborsRadius(grid, x, y, 2, CellState.forest) >= 4 && CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 8)
            {
                return CellState.grass;
            }

            // frontera grande de grass
            if (current == CellState.forest && counts[(int)CellState.grass] >= 2 && (CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 1))
            {
                return CellState.grass;
            }

            // expande forest
            if (current == CellState.grass && counts[(int)CellState.forest] >= 3 && CountNeighborsRadius(grid, x, y, 2, CellState.ocean) <= 1)
            {
                if (rng.NextDouble() < 0.2)
                    return CellState.forest;
            }

            // estabiliza forest
            if (current == CellState.forest && counts[(int)CellState.forest] >= 3)
            {
                return CellState.forest;
            }

            // expande ocean
            if (current == CellState.grass && CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 11)
            {
                if (rng.NextDouble() < 0.3)
                    return CellState.ocean;
            }

            // expande ocean
            if (current == CellState.mountain && counts[(int)CellState.ocean] >= 3)
            {
                if (rng.NextDouble() < 0.7)
                    return CellState.ocean;
            }

            if (!isFinalIteration)
            {
                // emergencia de dessert
                if (current == CellState.grass && (CountNeighborsRadius(grid, x, y, 2, CellState.ocean) + CountNeighborsRadius(grid, x, y, 2, CellState.forest)) == 0)
                {
                    if (rng.NextDouble() < 0.15)
                        return CellState.dessert;
                }
            }

            // expande dessert
            if (current == CellState.grass && counts[(int)CellState.dessert] >= 3)
            {
                if (rng.NextDouble() < 0.2)
                    return CellState.dessert;
            }

            // expande mountain
            if (current == CellState.beach && counts[(int)CellState.mountain] >= 4)
            {
                if (rng.NextDouble() < 0.25)
                    return CellState.mountain;
            }

            // frontera beach
            if (current == CellState.dessert && counts[(int)CellState.ocean] >= 1)
            {
                return CellState.beach;
            }

            // frontera grande de beach
            if (current == CellState.dessert && counts[(int)CellState.beach] >= 2 && (CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 1))
            {
                return CellState.beach;
            }

            // expande mountain
            if (current == CellState.dessert && counts[(int)CellState.mountain] >= 4)
            {
                if (rng.NextDouble() < 0.25)
                    return CellState.mountain;
            }

            // expande grass
            if (current == CellState.mountain && counts[(int)CellState.grass] >= 4 && (CountNeighborsRadius(grid, x, y, 2, CellState.mountain) <= 9))
            {
                return CellState.grass;
            }

            // estabiliza beach
            if (current == CellState.beach)
            {

                // si hay mucho océano cerca → permitir que ocean avance
                if (CountNeighborsRadius(grid, x, y, 2, CellState.ocean) >= 10)
                {
                        return CellState.ocean;
                }

                // si no toca océano → no es playa real
                if (CountNeighborsRadius(grid, x, y, 2, CellState.ocean) <= 1)
                    return GetMajorityStateRadius(grid, x, y, 1, current);


                // si hay mucha playa alrededor → se estabiliza
                if (CountNeighborsRadius(grid, x, y, 2, CellState.beach) >= 3)
                    return CellState.beach;

            }

            // frontera grass
            if (current == CellState.dessert && counts[(int)CellState.forest] >= 1)
            {
                return CellState.grass;
            }

            // frontera grande de grass
            if (current == CellState.dessert && counts[(int)CellState.grass] >= 3 && (CountNeighborsRadius(grid, x, y, 2, CellState.forest) >= 1))
            {
                return CellState.grass;
            }

            // ============================================================
            // 1. REGLA DE MAYORÍA (crea islas y regiones grandes)
            // ============================================================
                CellState majority = GetMajorityStateRadius(grid, x, y, 1, current);
            if (majority != current)
                    return majority;
            

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


