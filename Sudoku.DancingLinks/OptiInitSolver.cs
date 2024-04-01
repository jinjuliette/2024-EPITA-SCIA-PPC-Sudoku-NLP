using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class OptiInitSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.OptiInit dlx = new CustomDlxLib.OptiInit(s);
        dlx.Solve();
        return s;
    }
}