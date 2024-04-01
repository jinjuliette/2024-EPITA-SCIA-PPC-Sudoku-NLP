using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class LessLineSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.LessLines dlx = new CustomDlxLib.LessLines(s);
        dlx.Solve();
        return s;
    }
}