﻿/*
* PROJECT:          Atomix Development
* LICENSE:          BSD 3-Clause (LICENSE.md)
* PURPOSE:          Stind_I4 MSIL
* PROGRAMMERS:      Aman Priyadarshi (aman.eureka@gmail.com)
*/

using System.Reflection;

namespace Atomix.IL
{
    [ILOp(ILCode.Stind_I4)]
    public class Stind_I4 : MSIL
    {
        public Stind_I4(Compiler Cmp)
            : base("stindi4", Cmp) { }

        public override void Execute(ILOpCode instr, MethodBase aMethod)
        {
            Stind_I.Stind_All(4);
        }
    }
}
