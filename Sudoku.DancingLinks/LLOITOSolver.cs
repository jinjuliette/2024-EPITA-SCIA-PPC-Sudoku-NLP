using Sudoku.Shared;

namespace Sudoku.DancingLinks;

public class LLOITOSOlver : ISudokuSolver
{
    public SudokuGrid Solve(SudokuGrid s)
    {
        CustomDlxLib.LLOITO dlx = new CustomDlxLib.LLOITO(s);
        dlx.Solve();
        return s;
    }
}