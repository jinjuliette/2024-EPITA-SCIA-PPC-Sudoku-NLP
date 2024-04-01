using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class ParallelizedSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.ParallelizedDlx dlx = new CustomDlxLib.ParallelizedDlx(s);
        dlx.Solve();
        return s;
    }
}