using System;

namespace GameCore.Exceptions;
public class DialogException : Exception
{
    public DialogException(string message) : base(message)
    {
    }
}
