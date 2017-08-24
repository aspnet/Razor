// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    internal abstract class ForegroundDispatcher
    {
        public abstract bool IsForegroundThread { get; }

        public virtual void AssertForegroundThread([CallerMemberName] string caller = null)
        {

        }

        public abstract void AssertBackgroundThread([CallerMemberName] string caller = null)
        {

        }
    }
}
