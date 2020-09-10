// © 2019–2020 Endless OS Foundation LLC
//
// This file is part of Endless Launcher.
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
using System;
using System.Diagnostics.CodeAnalysis;

namespace EndlessLauncher.model
{
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable")]
    public abstract class EndlessExceptionBase<T> : Exception 
    {
        public EndlessExceptionBase(T errorCode, string message) : base(message) => Code = errorCode;

        public T Code
        {
            get;
            private set;
        }
    }
}
