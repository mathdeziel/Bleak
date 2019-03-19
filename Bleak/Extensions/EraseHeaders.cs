﻿using Bleak.Extensions.Interfaces;
using Bleak.Native;
using Bleak.Tools;
using Bleak.Wrappers;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Bleak.Extensions
{
    internal class EraseHeaders : IExtensionMethod
    {
        private readonly PropertyWrapper PropertyWrapper;

        internal EraseHeaders(PropertyWrapper propertyWrapper)
        {
            PropertyWrapper = propertyWrapper;
        }

        public bool Call()
        {
            var dllName = Path.GetFileName(PropertyWrapper.DllPath);

            // Look for an instance of the DLL in the target process

            var module = NativeTools.GetModuleInstance(NativeTools.GetProcessModules(PropertyWrapper.Process.Id), dllName);

            if (module.Equals(default(Structures.ModuleEntry)))
            {
                throw new ArgumentException($"Failed to find {dllName} in the target processes module list");
            }

            // Get the information about the header region of the DLL in the target process

            var memoryInformationBuffer = (IntPtr) PropertyWrapper.SyscallManager.InvokeSyscall<Syscall.Definitions.NtQueryVirtualMemory>(PropertyWrapper.ProcessHandle.Value, module.BaseAddress);

            // Marshal the information from the buffer

            var memoryInformation = Marshal.PtrToStructure<Structures.MemoryBasicInformation>(memoryInformationBuffer);

            // Write over the header region of the DLL with a buffer of zeroes

            var buffer = new byte[(int) memoryInformation.RegionSize];

            PropertyWrapper.MemoryManager.Value.WriteMemory(module.BaseAddress, buffer);

            return true;
        }
    }
}
