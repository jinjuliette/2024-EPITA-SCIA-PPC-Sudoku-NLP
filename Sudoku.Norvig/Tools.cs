namespace Sudoku.Norvig;

public class Tools
{
    public const int SIZE = 9;
    public const int SURFACE = 81;
#if false
    public static void FillPeers(HashSet<int>[] peers)
    {
        Parallel.For(0, SURFACE, (i, state) =>
        {
            HashSet<int> set = new HashSet<int>();
            AddColumn(i, set);
            AddRow(i, set);
            AddSquare(i, set);
            set.Remove(i);
            peers[i] = set;
        });
    }

    private static void AddRow(int cell, HashSet<int> set)
    {
        int inferiorLimit = cell / SIZE * SIZE;
        int superiorLimit = cell / SIZE * (SIZE + 1);
        for (int i = inferiorLimit; i < superiorLimit; i++)
            set.Add(i);
    }
    
    private static void AddSquare(int cell, HashSet<int> set)
    {
        int coordinateX = (cell / SIZE) / 3 * 3;
        int coordinateY = (cell % SIZE) / 3 * 3;
        int inferiorLimit = coordinateY * SIZE + coordinateX;
        for (int i = 0; i < SIZE; i++)
        {
            int target = inferiorLimit + (i / 3) * SIZE + (i % 3);
            set.Add(target);
        }
    }
    
    private static void AddColumn(int cell, HashSet<int> set)
    {
        int inferiorLimit = cell % SIZE;
        for (int i = inferiorLimit; i < SURFACE; i += SIZE)
            set.Add(i);
    }
#endif
}