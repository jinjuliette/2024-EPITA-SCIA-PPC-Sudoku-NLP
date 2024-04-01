using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class TypeOptiSolver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.TypeOpti dlx = new CustomDlxLib.TypeOpti(s);
        dlx.Solve();
        return s;
    }
}