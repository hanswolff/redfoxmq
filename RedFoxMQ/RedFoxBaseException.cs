using System;

namespace RedFoxMQ
{
    public abstract class RedFoxBaseException : Exception
    {
        protected RedFoxBaseException()
        {
        }

        protected RedFoxBaseException(string message)
            : base(message)
        {
        }
    }
}
