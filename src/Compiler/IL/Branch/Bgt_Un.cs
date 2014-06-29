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
    [ILOp(ILCode.Bgt_Un)]
    public class Bgt_Un : MSIL
    {
        public Bgt_Un(Compiler Cmp)
            : base("bgt_un", Cmp) { }

        public override void Execute(ILOpCode instr, MethodBase aMethod)
        {
            //This is branch type IL
            var xOffset = ((OpBranch)instr).Value;
            var xSize = Core.vStack.Pop().Size;
            var xTrueLabel = ILHelper.GetLabel(aMethod, xOffset);
            var xFalseLabel = xTrueLabel + "._bgt_un_false";

            //Make a pop because exactly we are popping up two items
            Core.vStack.Pop();

            /*
                value1 is pushed onto the stack.
                value2 is pushed onto the stack.
                value2 and value1 are popped from the stack;
                if value1 is greater than value2, the branch operation is performed. --> value1 > value2 
            */
            switch (ILCompiler.CPUArchitecture)
            {
                #region _x86_
                case CPUArch.x86:
                    {
                        if (xSize <= 4)
                        {
                            //***What we are going to do is***
                            //1) Pop value 2 into EAX
                            //2) Pop value 1 into EBX
                            //3) Compare EBX and EAX and jump if greator than
                            Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EAX });//Value 2
                            Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EBX });//Value 1
                            Core.AssemblerCode.Add(new Cmp { DestinationReg = Registers.EBX, SourceReg = Registers.EAX });
                            Core.AssemblerCode.Add(new Jmp { Condition = ConditionalJumpEnum.JA, DestinationRef = xTrueLabel });
                        }
                        else if (xSize <= 8)
                        {
                            //***What we are going to do is***
                            //1) Pop value 2 low into EAX and high into EBX
                            //2) Pop value 1 low into ECX and high into EDX
                            //3) Compare High parts of value 1 and value 2, if greator than true else if less than jump false
                            //4) Compare Low parts of value 1 and value 2, if greator than jump true else continue

                            //Value 2 EBX:EAX                            
                            Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EAX });//Low
                            Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EBX });//High
                            
                            //Value 1 EDX:ECX
                            Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.ECX });//Low
                            Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.EDX });//High

                            Core.AssemblerCode.Add(new Cmp { DestinationReg = Registers.EDX, SourceReg = Registers.EBX });//value1_HI - value2_HI                            
                            Core.AssemblerCode.Add(new Jmp { Condition = ConditionalJumpEnum.JA, DestinationRef = xTrueLabel });
                            Core.AssemblerCode.Add(new Jmp { Condition = ConditionalJumpEnum.JB, DestinationRef = xFalseLabel });
                            Core.AssemblerCode.Add(new Cmp { DestinationReg = Registers.ECX, SourceReg = Registers.EAX });//value1_LO - value2_LO
                            Core.AssemblerCode.Add(new Jmp { Condition = ConditionalJumpEnum.JA, DestinationRef = xTrueLabel });

                            Core.AssemblerCode.Add(new Label(xFalseLabel));                            
                        }
                        else
                            //Not called usually
                            throw new Exception("@Bgt: Branch operation bgt for size > 8 is not yet implemented");
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
