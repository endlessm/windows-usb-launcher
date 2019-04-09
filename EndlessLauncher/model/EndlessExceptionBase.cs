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
