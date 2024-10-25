using Sudoku.Shared;
using Microsoft.Z3;

public class Z3_V5_BitVect_Mask_Tactics_Subs : Z3Optimization
{
    private static readonly int Size = 9;
    private static readonly int BlockSize = 3;
    private static Context ctx;
    private static Solver solver;
    private static BitVecExpr[][] X;
    private static List<BoolExpr> genericConstraints;

    // Initialization of the Z3 context and solver, as well as the evaluation matrix
    static Z3_V5_BitVect_Mask_Tactics_Subs()
    {
        ctx = new Context(new Dictionary<string, string> { { "model", "true" } });
        solver = ctx.MkSolver();
        X = CreateEvalMatrix();
        genericConstraints = GenerateConstraints();
    }

    // Entry point of the Sudoku solver
    public override SudokuGrid solve(SudokuGrid s)
    {
        ResetSolver();

        // Generates the constraints of the Sudoku
        var constraints = new List<BoolExpr>(genericConstraints); // Use generic constraints

        // Adds the constraints related to the Sudoku instance given as input
        AddSudokuInstanceConstraints(s, ref constraints);

        // Adds the constraints to the solver and solves it
        solver.Assert(constraints.ToArray());

        // Apply tactics to improve solving efficiency
        ApplyTactics();

        if (solver.Check() != Status.SATISFIABLE)
            return s;

        return ExtractSolution();
    }

    private static void ResetSolver()
    {
        solver.Reset();
    }

    private static BitVecExpr[][] CreateEvalMatrix()
    {
        // Creates the evaluation matrix X by initializing each cell with a 4-bit binary variable
        return Enumerable.Range(0, Size)
                            .Select(i => Enumerable.Range(0, Size)
                                                .Select(j => ctx.MkBVConst($"x_{i + 1}_{j + 1}", 4))
                                                .ToArray())
                            .ToArray();
    }

    private static List<BoolExpr> GenerateConstraints()
    {
        // Generates the different constraints of the Sudoku
        return new List<BoolExpr>
        {
            GenerateCellConstraints(),
            GenerateRowConstraints(),
            GenerateColumnConstraints(),
            GenerateBlockConstraints()
        };
    }

    private static BoolExpr GenerateCellConstraints()
    {
        // Each cell must contain a value between 1 and 9
        var one = ctx.MkBV(1, 4);
        var nine = ctx.MkBV(9, 4);

        return ctx.MkAnd(X.SelectMany(row =>
            row.Select(cell => ctx.MkAnd(ctx.MkBVULE(one, cell), ctx.MkBVULE(cell, nine)))
        ).ToArray());
    }

    private static BoolExpr GenerateRowConstraints()
    {
        // Each row must not contain duplicates
        return ctx.MkAnd(X.Select(row => ctx.MkDistinct(row)).ToArray());
    }

    private static BoolExpr GenerateColumnConstraints()
    {
        // Each column must not contain duplicates
        return ctx.MkAnd(Enumerable.Range(0, Size).Select(j =>
            ctx.MkDistinct(Enumerable.Range(0, Size).Select(i => X[i][j]).ToArray())
        ).ToArray());
    }

    private static BoolExpr GenerateBlockConstraints()
    {
        // Each 3x3 subgrid must not contain duplicates
        return ctx.MkAnd(Enumerable.Range(0, BlockSize).SelectMany(i =>
            Enumerable.Range(0, BlockSize).Select(j =>
                ctx.MkDistinct(Enumerable.Range(0, BlockSize).SelectMany(di =>
                    Enumerable.Range(0, BlockSize).Select(dj => X[BlockSize * i + di][BlockSize * j + dj])
                ).ToArray())
            )
        ).ToArray());
    }

    private static void AddSudokuInstanceConstraints(SudokuGrid s, ref List<BoolExpr> constraints)
    {
        // Adds the constraints related to the Sudoku instance given as input
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                if (s.Cells[i, j] != 0)
                    constraints.Add(ctx.MkEq(X[i][j], ctx.MkBV(s.Cells[i, j], 4)));
    }

    private static void ApplyTactics()
    {
        // Define tactics and apply them to the solver
        var tactic = ctx.MkTactic("simplify");
        var goal = ctx.MkGoal();
        goal.Assert(solver.Assertions);
        var result = tactic.Apply(goal);

        solver.Assert(result.Subgoals[0].Formulas);
    }

    private static SudokuGrid ExtractSolution()
    {
        // Extracts the Sudoku solution from the model returned by the solver
        var sudokuGrid = new SudokuGrid();

        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
            {
                var eval = solver.Model.Evaluate(X[i][j]) as BitVecNum;
                sudokuGrid.Cells[i, j] = (int)eval.UInt64;
            }

        return sudokuGrid;
    }
}