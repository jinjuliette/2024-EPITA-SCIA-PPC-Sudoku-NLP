using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class LLOISolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.LessLineOptiInit dlx = new CustomDlxLib.LessLineOptiInit(s);
        dlx.Solve();
        return s;
    }
}