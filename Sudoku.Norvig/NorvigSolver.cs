﻿using System.IO.Pipes;
using Sudoku.Shared;

namespace Sudoku.Norvig;

public class NorvigSolver : ISudokuSolver
{
    private short[] _possibleValues; // a 1D array that represent all possible value of a given cell of the sudoku


    public NorvigSolver()
    {
        _possibleValues = new short[Tools.SURFACE];
    }

    public SudokuGrid Solve(SudokuGrid s)
    {
        // Reset of grid: every cell has every number as a possible value.
        Parallel.For(0, Tools.SURFACE, (i, state) => _possibleValues[i] = 0x1FF);
        Constrain(s);

        if (!NoMorePossibleValues())
            Search();

        FillGrid(s);
        return s;
    }

    private bool Search()
    {
        
        // we search the cell that has the least possible value
        int cell = FindCellWithLeastPossibilities();

        // if no cell has multiple possibilites, the search has succeeded
        if (cell == -1)
            return true;
        
        Console.WriteLine(Tools.Picture(_possibleValues));

        for (short i = 1; i < 0x1FF; i = (short)(i << 1))
        {
            // ignore all i which are not a possible value of the cell
            if ((_possibleValues[cell] & i) == 0)
                continue;

            int digit = Tools.ConvertBitwiseToDecimal(i);

            short[] save = (short[])_possibleValues.Clone();
            
            Fill(cell, digit);
            bool hasSucceeded = Search();
            if (hasSucceeded)
                return true;
            
            // else we use our save to restate our _possible values to the original
            _possibleValues = save;
        }

        return false;
    }

    private int FindCellWithLeastPossibilities()
    {
        uint lowestNbBitsSet = 9;
        int highestCell = -1;
        for (int cell = 0; cell < Tools.SURFACE; cell++)
        {
            // find the number of bits equals to one = find the number of possible values of cell
            uint setBits = System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)_possibleValues[cell]);

            if (setBits < lowestNbBitsSet && setBits != 1)
            {
                highestCell = cell;
                lowestNbBitsSet = setBits;
            }
        }

        return highestCell;
    }

    private bool NoMorePossibleValues()
    {
        for (int i = 0; i < Tools.SURFACE; i++)
            if (!Tools.IsOnlyOneBitSet(_possibleValues[i]))
                return false;

        return true;
    }

    private void FillGrid(SudokuGrid s)
    {
        for (int i = 0; i < Tools.SIZE; i++)
        for (int j = 0; j < Tools.SIZE; j++)
        {
            if (!Tools.IsOnlyOneBitSet(_possibleValues[i * Tools.SIZE + j]))
                s.Cells[i, j] = 0;
            else
                s.Cells[i, j] = Tools.ConvertBitwiseToDecimal(_possibleValues[i * Tools.SIZE + j]);
        }
    }

    private void Constrain(SudokuGrid s)
    {
        for (int i = 0; i < Tools.SIZE; i++)
        {
            for (int j = 0; j < Tools.SIZE; j++)
            {
                int cell = i * Tools.SIZE + j;
                // if a cell of the given sudoku grid is not empty (i.e. a value is already set)
                // we fill our grid of possible values at that cell
                if (s.Cells[i, j] != 0)
                {
                    Fill(cell, s.Cells[i, j]);
                }
            }
        }
    }

    private bool Fill(int cell, int digit)
    {
        if (Tools.IsOnlyOneBitSet(_possibleValues[cell]) && Tools.ConvertBitwiseToDecimal(_possibleValues[cell]) == digit)
            return true;

        bool eliminated = true;

        // is equivalent to the `all` function in python
        for (int newDigit = 1; newDigit < 10; newDigit++)
        {
            if (newDigit == digit)
                continue;

            eliminated = Eliminate(cell, newDigit) && eliminated;
        }

        return eliminated;
    }
    
    private bool Eliminate(int cell, int digit)
    {
        // bitmask in order to select the (digit - 1) bit
        short bitmask = (short)(0x1 << (digit - 1));

        // if we already eliminated the digit, we return true
        // as to say we successfully eliminated it already
        if ((_possibleValues[cell] & bitmask) == 0)
            return true;

        short inverseBitmask = (short)(bitmask ^ 0x1FF);
        
        // we eliminate the bit that represents `digit`
        short newPossibleValues = (short)(_possibleValues[cell] & inverseBitmask);

        _possibleValues[cell] = newPossibleValues;
        
        // if there is no possible value left
        // that is not a legal elimination
        if (newPossibleValues == 0)
            return false;


        // if there is only one possibility
        // we eliminate that possibility for each of the cell's peers
        if (Tools.IsOnlyOneBitSet(newPossibleValues))
        {
            var peers = Tools.peers[cell];

            int digit2 = Tools.ConvertBitwiseToDecimal(newPossibleValues);

            bool eliminated = true;

            foreach (int cellNeighbour in peers)
            {
                eliminated = Eliminate(cellNeighbour, digit2) && eliminated;
            }

            if (!eliminated)
                return false;
        }

        for (int i = 0; i < 3; i++)
        {
            HashSet<int> unit = Tools.units[cell, i];
            // construction of the list of units that has `digit` as a possibility
            IEnumerable<int> possibleDigitPlaces = unit.Where(x => (bitmask & _possibleValues[x]) != 0);
            if (!possibleDigitPlaces.Any() ||
                (possibleDigitPlaces.Count() == 1 && !Fill(possibleDigitPlaces.First(), digit)))
            {
                return false;
            }
        }

        return true;
    }
}