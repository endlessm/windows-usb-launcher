using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EndlessLauncher.model
{
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
