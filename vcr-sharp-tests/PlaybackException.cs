using System;

namespace VcrSharp.Tests
{
    public class PlaybackException : Exception
    {
        public PlaybackException(string message) : base(message)
        {
        }
    }
}
