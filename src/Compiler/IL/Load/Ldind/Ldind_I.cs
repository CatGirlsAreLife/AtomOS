﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Atomix.IL;
using Atomix.Assembler;
using Atomix.Assembler.x86;
using Atomix.CompilerExt;
using System.Reflection;
using Atomix.ILOpCodes;
using Core = Atomix.Assembler.AssemblyHelper;

namespace Atomix.IL
{
    [ILOp(ILCode.Ldind_I)]
    public class Ldind_I : MSIL
    {
        public Ldind_I(Compiler Cmp)
            : base("ldind_i", Cmp) { }

        public override void Execute(ILOpCode instr, MethodBase aMethod)
        {
            switch (ILCompiler.CPUArchitecture)
            {
                #region _x86_
                case CPUArch.x86:
                    {
                        Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EAX });
                        Core.AssemblerCode.Add(new Push { DestinationReg = Registers.EAX, DestinationIndirect = true });
                    }
                    break;
                #endregion
                #region _x64_
                case CPUArch.x64:
                    {

                    }
                    break;
                #endregion
                #region _ARM_
                case CPUArch.ARM:
                    {

                    }
                    break;
                #endregion
            }
        }
    }
}
