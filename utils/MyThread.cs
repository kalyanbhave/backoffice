//==================================================================== 
// Credit Card Encryption/Decryption Tool
// 
// Copyright (c) 2009-2015 Egencia.  All rights reserved. 
// This software was developed by Egencia An Expedia Inc. Corporation
// La Defense. Paris. France
// The Original Code is Egencia 
// The Initial Developer is Samatar Hassan.
//
//===================================================================

using System;
using System.Data;
using System.Threading;
using System.Runtime.CompilerServices;
using SafeNetWS.log;
using System.DirectoryServices;
using System.Collections.Generic;

namespace SafeNetWS.utils
{
    public class MyThread
    {

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Filelog VolatileRead(ref Filelog address)
        {
            Filelog result = address;
            Thread.MemoryBarrier();
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string VolatileRead(ref string address)
        {
            string result = address;
            Thread.MemoryBarrier();
            return result;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<string, string> VolatileRead(ref Dictionary<string, string> address)
        {
            Dictionary<string, string> result = address;
            Thread.MemoryBarrier();
            return result;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static DirectoryEntry VolatileRead(ref DirectoryEntry address)
        {
            DirectoryEntry result = address;
            Thread.MemoryBarrier();
            return result;
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int VolatileRead(ref int address)
        {
            int result = address;
            Thread.MemoryBarrier();
            return result;
        }
    }
}
