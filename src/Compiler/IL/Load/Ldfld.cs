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
    [ILOp(ILCode.Ldfld)]
    public class Ldfld : MSIL
    {
        public Ldfld(Compiler Cmp)
            : base("ldfld", Cmp) { }

        public override void Execute(ILOpCode instr, MethodBase aMethod)
        {
            /* Pop the object first */
            var xStackValue = Core.vStack.Pop();

            var xF = ((OpField)instr).Value;
            var aDeclaringType = xF.DeclaringType;
            var xFieldId = xF.FullName();

            FieldInfo xFieldInfo = null;
            
            //Now we have to calculate the offset of object, and also give us that field
            int xOffset = ILHelper.GetFieldOffset(aDeclaringType, xFieldId, out xFieldInfo);
            bool xNeedsGC = aDeclaringType.IsClass && !aDeclaringType.IsValueType;
            if (xNeedsGC)
                xOffset += 12; //Extra offset =)

            //As we are sure xFieldInfo should contain value as if not than it throws error in GetFieldOffset
            var xSize = xFieldInfo.FieldType.SizeOf();
            
            switch (ILCompiler.CPUArchitecture)
            {
                #region _x86_
                case CPUArch.x86:
                    {
                        Core.AssemblerCode.Add(new Pop { DestinationReg = Registers.ECX });
                        Core.AssemblerCode.Add(new Add { DestinationReg = Registers.ECX, SourceRef = "0x" + xOffset.ToString("X") });

                        for (int i = 1; i <= (xSize / 4); i++)
                        {
                            Core.AssemblerCode.Add(new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.ECX, SourceIndirect = true, SourceDisplacement = (int)(xSize - (i * 4)) });
                            Core.AssemblerCode.Add(new Push { DestinationReg = Registers.EAX });
                        }
                        switch (xSize % 4)
                        {
                            case 1:
                                {
                                    Core.AssemblerCode.Add(new Mov { DestinationReg = Registers.EAX, SourceRef = "0x0" });
                                    Core.AssemblerCode.Add(new Mov { DestinationReg = Registers.AL, SourceReg = Registers.ECX, SourceIndirect = true, Size = 8 });
                                    Core.AssemblerCode.Add(new Push { DestinationReg = Registers.EAX });
                                    break;
                                }
                            case 2:
                                {
                                    Core.AssemblerCode.Add(new Mov { DestinationReg = Registers.EAX, SourceRef = "0x0" });
                                    Core.AssemblerCode.Add(new Mov { DestinationReg = Registers.AX, SourceReg = Registers.ECX, SourceIndirect = true, Size = 16 });
                                    Core.AssemblerCode.Add(new Push { DestinationReg = Registers.EAX });
                                    break;
                                }

                            case 3: //For Release
                                {
                                    Core.AssemblerCode.Add(new Mov { DestinationReg = Registers.EAX, SourceRef = "0x0" });
                                    Core.AssemblerCode.Add(new Mov { DestinationReg = Registers.EAX, SourceReg = Registers.ECX, SourceIndirect = true });
                                    Core.AssemblerCode.Add(new Shr { DestinationReg = Registers.EAX, SourceRef = "0x8" });
                                    Core.AssemblerCode.Add(new Push { DestinationReg = Registers.EAX });
                                    break;
                                }
                            case 0:
                                {
                                    break;
                                }
                            default:
                                throw new Exception("Remainder size not supported!");
                        }
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
            Core.vStack.Push(xF.FieldType.SizeOf().Align(), xF.FieldType);
        }
    }
}
