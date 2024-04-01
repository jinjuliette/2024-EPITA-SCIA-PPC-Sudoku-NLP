using Sudoku.Shared;

namespace CustomDlxLib
{
    public class OptiInit
    {
        private SudokuGrid s;
        private ColumnNode root;
        private LinkedList<Node> solution = [];

        public OptiInit(SudokuGrid s)
        {
            this.s = s;
        }

        public void Solve()
        {
            var start = DateTime.Now;
            Init();
            var initTime = (DateTime.Now - start).TotalMilliseconds;
            
            start = DateTime.Now;
            Search();
            var searchTime = (DateTime.Now - start).TotalMilliseconds;
            
            start = DateTime.Now;
            foreach (Node node in solution)
            {
                int value = node.RowIndex % 9;
                int i = (node.RowIndex / 9) % 9;
                int j = node.RowIndex / 81;
                s.Cells[i, j] = value + 1;
            }
            var convertTime = (DateTime.Now - start).TotalMilliseconds;

            using var file = new StreamWriter("opti init_time.csv", true);
            file.WriteLine($"{initTime},{searchTime},{convertTime}");
        }

        private void Init()
        {
            root = new ColumnNode();
            root.Left = root;
            root.Right = root;

            Node c = root;
            ColumnNode[] columnsNodes = new ColumnNode[324];
            int columnsAppenderIdx = 0;

            // create row column constraints
            for (int i = 0; i < 324; i++)
            {
                ColumnNode newColumn = new ColumnNode();
                columnsNodes[columnsAppenderIdx++] = newColumn;
                newColumn.Up = newColumn;
                newColumn.Down = newColumn;

                c.Right = newColumn;
                newColumn.Left = c;

                c = newColumn;
            }
            columnsNodes[323].Right = root;
            root.Left = columnsNodes[323];

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    int blockIndex = i / 3 + j / 3 * 3;
                    int singleColumnIndex = 9 * j + i;

                    int value = s.Cells[i, j] - 1;
                    int rowIndex = 81 * j + 9 * i + value;
                    int rowNumberConstraintIndex = 81 + 9 * j;
                    int columnNumberConstraintIndex = 162 + 9 * i;
                    int boxNumberConstraintIndex = 243 + blockIndex * 9;

                    if (value >= 0)
                    {
                        var rcColumnNode = columnsNodes[singleColumnIndex];
                        var rnColumnNode = columnsNodes[rowNumberConstraintIndex + value];
                        var cnColumnNode = columnsNodes[columnNumberConstraintIndex + value];
                        var bnColumnNode = columnsNodes[boxNumberConstraintIndex + value];

                        rcColumnNode.Size++;
                        rnColumnNode.Size++;
                        cnColumnNode.Size++;
                        bnColumnNode.Size++;


                        // always 4 1s in the row RC, RN, CN, BN
                        var rcNode = new Node(rcColumnNode, rowIndex);
                        var rnNode = new Node(rnColumnNode, rowIndex);
                        var cnNode = new Node(cnColumnNode, rowIndex);
                        var bnNode = new Node(bnColumnNode, rowIndex);

                        rcNode.Left = bnNode;
                        rcNode.Right = rnNode;

                        rnNode.Left = rcNode;
                        rnNode.Right = cnNode;

                        cnNode.Left = rnNode;
                        cnNode.Right = bnNode;

                        bnNode.Left = cnNode;
                        bnNode.Right = rcNode;

                        rcNode.Down = rcColumnNode;
                        rcNode.Up = rcColumnNode.Up;
                        rcColumnNode.Up.Down = rcNode;
                        rcColumnNode.Up = rcNode;

                        rnNode.Down = rnColumnNode;
                        rnNode.Up = rnColumnNode.Up;
                        rnColumnNode.Up.Down = rnNode;
                        rnColumnNode.Up = rnNode;

                        cnNode.Down = cnColumnNode;
                        cnNode.Up = cnColumnNode.Up;
                        cnColumnNode.Up.Down = cnNode;
                        cnColumnNode.Up = cnNode;

                        bnNode.Down = bnColumnNode;
                        bnNode.Up = bnColumnNode.Up;
                        bnColumnNode.Up.Down = bnNode;
                        bnColumnNode.Up = bnNode;
                    }
                    else
                    {
                        for (int d = 0; d < 9; d++)
                        {
                            rowIndex = 81 * j + 9 * i + d;

                            var rcColumnNode = columnsNodes[singleColumnIndex];
                            var rnColumnNode = columnsNodes[rowNumberConstraintIndex + d];
                            var cnColumnNode = columnsNodes[columnNumberConstraintIndex + d];
                            var bnColumnNode = columnsNodes[boxNumberConstraintIndex + d];

                            rcColumnNode.Size++;
                            rnColumnNode.Size++;
                            cnColumnNode.Size++;
                            bnColumnNode.Size++;

                            // always 4 1s in the row RC, RN, CN, BN
                            var rcNode = new Node(rcColumnNode, rowIndex);
                            var rnNode = new Node(rnColumnNode, rowIndex);
                            var cnNode = new Node(cnColumnNode, rowIndex);
                            var bnNode = new Node(bnColumnNode, rowIndex);

                            rcNode.Left = bnNode;
                            rcNode.Right = rnNode;

                            rnNode.Left = rcNode;
                            rnNode.Right = cnNode;

                            cnNode.Left = rnNode;
                            cnNode.Right = bnNode;

                            bnNode.Left = cnNode;
                            bnNode.Right = rcNode;


                            rcNode.Down = rcColumnNode;
                            rcNode.Up = rcColumnNode.Up;
                            rcColumnNode.Up.Down = rcNode;
                            rcColumnNode.Up = rcNode;

                            rnNode.Down = rnColumnNode;
                            rnNode.Up = rnColumnNode.Up;
                            rnColumnNode.Up.Down = rnNode;
                            rnColumnNode.Up = rnNode;


                            cnNode.Down = cnColumnNode;
                            cnNode.Up = cnColumnNode.Up;
                            cnColumnNode.Up.Down = cnNode;
                            cnColumnNode.Up = cnNode;

                            bnNode.Down = bnColumnNode;
                            bnNode.Up = bnColumnNode.Up;
                            bnColumnNode.Up.Down = bnNode;
                            bnColumnNode.Up = bnNode;
                        }
                    }
                }
            }
        }

        private void Cover(Node c)
        {
            c.Right.Left = c.Left;
            c.Left.Right = c.Right;
            for (Node i = c.Down; i != c; i = i.Down)
            {
                for (Node j = i.Right; j != i; j = j.Right)
                {
                    j.Down.Up = j.Up;
                    j.Up.Down = j.Down;
                    j.Column.Size--;
                }
            }
        }

        private void Uncover(Node c)
        {
            for (var i = c.Up; i != c; i = i.Up)
            {
                for (var j = i.Left; j != i; j = j.Left)
                {
                    j.Column.Size++;
                    j.Down.Up = j;
                    j.Up.Down = j;
                }
            }

            c.Right.Left = c;
            c.Left.Right = c;
        }

        private bool Search()
        {
            if (root.Right == root)
            {
                return true;
            }

            ColumnNode selected = (ColumnNode)root.Right;
            // int sum = 0;
            for (ColumnNode i = (ColumnNode)root.Right; i != root; i = (ColumnNode)i.Right)
            {
                // sum += i.Size;
                if (i.Size < selected.Size)
                {
                    selected = i;
                }
            }
            // Console.WriteLine(sum);

            Cover(selected);

            for (Node i = selected.Down; i != selected; i = i.Down)
            {
                solution.AddLast(i);

                for (Node j = i.Right; j != i; j = j.Right)
                {
                    Cover(j.Column);
                }

                if (Search())
                {
                    return true;
                }

                solution.RemoveLast();

                for (Node j = i.Left; j != i; j = j.Left)
                {
                    Uncover(j.Column);
                }
            }

            Uncover(selected);

            return false;
        }

        public class Node
        {
            public Node Left;
            public Node Right;
            public Node Up;
            public Node Down;
            public readonly ColumnNode Column;
            public readonly int RowIndex;

            public Node()
            {
            }

            public Node(ColumnNode column, int rowIndex)
            {
                Column = column;
                RowIndex = rowIndex;
            }
        }

        public class ColumnNode : Node
        {
            internal int Size;
        }
    }
}