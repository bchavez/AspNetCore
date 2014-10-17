﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Security.DataProtection.Cng;

namespace Microsoft.AspNet.Security.DataProtection.SP800_108
{
    /// <summary>
    /// Provides an implementation of the SP800-108-CTR-HMACSHA512 key derivation function.
    /// This class assumes at least Windows 7 / Server 2008 R2.
    /// </summary>
    /// <remarks>
    /// More info at http://csrc.nist.gov/publications/nistpubs/800-108/sp800-108.pdf, Sec. 5.1.
    /// </remarks>
    internal unsafe static class SP800_108_CTR_HMACSHA512Util
    {
        // Creates a provider with an empty key.
        public static ISP800_108_CTR_HMACSHA512Provider CreateEmptyProvider()
        {
            byte dummy;
            return CreateProvider(pbKdk: &dummy, cbKdk: 0);
        }

        // Creates a provider from the given key.
        public static ISP800_108_CTR_HMACSHA512Provider CreateProvider(byte* pbKdk, uint cbKdk)
        {
            if (OSVersionUtil.IsBCryptOnWin8OrLaterAvailable())
            {
                return new Win8SP800_108_CTR_HMACSHA512Provider(pbKdk, cbKdk);
            }
            else
            {
                return new Win7SP800_108_CTR_HMACSHA512Provider(pbKdk, cbKdk);
            }
        }

        // Creates a provider from the given secret.
        public static ISP800_108_CTR_HMACSHA512Provider CreateProvider(ProtectedMemoryBlob kdk)
        {
            uint secretLengthInBytes = checked((uint)kdk.Length);
            if (secretLengthInBytes == 0)
            {
                return CreateEmptyProvider();
            }
            else
            {
                fixed (byte* pbPlaintextSecret = new byte[secretLengthInBytes])
                {
                    try
                    {
                        kdk.WriteSecretIntoBuffer(pbPlaintextSecret, checked((int)secretLengthInBytes));
                        return CreateProvider(pbPlaintextSecret, secretLengthInBytes);
                    }
                    finally
                    {
                        UnsafeBufferUtil.SecureZeroMemory(pbPlaintextSecret, secretLengthInBytes);
                    }
                }
            }
        }
    }
}
