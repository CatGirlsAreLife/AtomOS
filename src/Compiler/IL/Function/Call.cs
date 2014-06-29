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
    [ILOp(ILCode.Call)]
    public class ILCall : MSIL
    {
        public ILCall(Compiler Cmp)
            : base("call", Cmp) { }

        public override void Execute(ILOpCode instr, MethodBase aMethod)
        {
            //The Target method base which we have to call
            var xTarget = ((OpMethod)instr).Value;
            //Try to get if there is any method info of target method
            var xTargetInfo = xTarget as MethodInfo;
            var xNormalAddress = xTarget.FullName(); //Target method full name
            var xParams = xTarget.GetParameters();            
            var xNextAddress = ILHelper.GetLabel(aMethod, instr.NextPosition);//Next address after call IL instruction

            /* Now to find error handling label 
               This is because if the called method throwed any exception than to handle that we need something. 
             */
            var xEndException = aMethod.FullName() + ".Error";//Current method exception label
            //Check if there is any try catch field after this call instruction
            if (instr.Ehc != null && instr.Ehc.HandlerOffset > instr.Position)
            {
                //If yes take that label as excpetion handling label
                xEndException = ILHelper.GetLabel(aMethod, instr.Ehc.HandlerOffset);
            }

            //If method have any method info than check what is its return size
            //And also align it to stack pointer
            var xReturnSize = 0;
            if (xTargetInfo != null)
                xReturnSize = xTargetInfo.ReturnType.SizeOf().Align();
            
            /* Size to reserve can be viewed as the segment of stack that is require to call a function
             * where function can put it all its parameters/local variables
             * And finally the return size */
            #region Size2Reserve
            int SizeToReserve = xReturnSize;
            if (SizeToReserve > 0)
            {
                foreach (var xp in xTarget.GetParameters())
                {
                    SizeToReserve -= xp.ParameterType.SizeOf().Align();
                    Core.vStack.Pop();
                }
                if (!xTargetInfo.IsStatic)
                {
                    SizeToReserve -= (ILCompiler.CPUArchitecture == CompilerExt.CPUArch.x86 ? 4 : 8);
                    Core.vStack.Pop();
                }
            }
            #endregion
            
            /*
                Method arguments arg1 through argN are pushed onto the stack.
                Method arguments arg1 through argN are popped from the stack;
                the method call is performed with these arguments and control is transferred,
                to the method referred to by the method descriptor. When complete, 
                a return value is generated by the callee method and sent to the caller.
                The return value is pushed onto the stack.
            */
            
            switch (ILCompiler.CPUArchitecture)
            {
                #region _x86_
                case CPUArch.x86:
                    {
                        //Make Space for extra requirements of function
                        if (SizeToReserve > 0)
                            Core.AssemblerCode.Add(new Sub { DestinationReg = Registers.ESP, SourceRef = SizeToReserve.ToString("X") });

                        Core.AssemblerCode.Add(new Call(xNormalAddress));

                        if (aMethod != null)
                        {
                            //Check if ECX register has no error value that is 0x1
                            //If yes than jump to next instruction
                            Core.AssemblerCode.Add(new Test { DestinationReg = Registers.ECX, SourceRef = "0x2" });
                            Core.AssemblerCode.Add(new Jmp { Condition = ConditionalJumpEnum.JE, DestinationRef = xNextAddress });

                            //If they are not equal it mean throwed exception, 
                            //then jump to next exception & firstly change the stack frame back to original 
                            //one as we are not going to return anything from this method
                            for (int i = 0; i < xReturnSize / 4; i++)
                                Core.AssemblerCode.Add(new Add { DestinationReg = Registers.ESP, SourceRef = "0x4" });
                            Core.AssemblerCode.Add(new Jmp { Condition = ConditionalJumpEnum.JNE, DestinationRef = xEndException });
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
            ///If there is not method info than why push anything to virtual stack
            if (xTargetInfo == null || xReturnSize == 0)
                return;
            
            Core.vStack.Push(xReturnSize, xTargetInfo.ReturnType);
        }
    }
}
