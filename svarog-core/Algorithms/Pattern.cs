using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace svarog.Algorithms
{
    public enum EPattern
    {
        F, T, _
    }

    public class IntPattern3x3
    {
        public readonly EPattern[,] Matrix;
        private readonly HashSet<(byte, byte)> Whatevers = new();

        public bool IsImportant(byte x, byte y) => !Whatevers.Contains((x, y));

        public IntPattern3x3(EPattern[,] pars)
        {
            Debug.Assert(pars.Length == 9);
            Debug.Assert(pars.Rank == 2);
            Matrix = pars;

            for (byte i = 0; i < 3; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    if (pars[i, j] == EPattern._)
                    {
                        Whatevers.Add((i, j));
                    }
                }
            }
        }

        public IntPattern3x3(string mapping)
        {
            var input = mapping.Trim().Where(c => c != '\n' && c != '\r').Select(c =>
            {
                if (c == 'T') { return EPattern.T; }
                else if (c == 'F') { return EPattern.F; }
                else if (c == '_') { return EPattern._; }
                else { throw new Exception($"No pattern {c} found!"); }
            }).ToList();

            Matrix = new EPattern[3, 3];

            for (byte i = 0; i < 3; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    Matrix[j, i] = input[i * 3 + j];
                    if (Matrix[j, i] == EPattern._)
                    {
                        Whatevers.Add((j, i));
                    }
                }
            }
        }
    }
}
