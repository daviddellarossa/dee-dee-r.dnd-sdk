using DeeDeeR.DnD.Core.Enums;
using DeeDeeR.MessageBus.Runtime.Core;

namespace DeeDeeR.DnD.Runtime.Bus.Args
{
    /// <summary>Published at the start and end of a short or long rest.</summary>
    public readonly struct RestArgs
    {
        public readonly EndpointId Character;
        public readonly RestType   Rest;

        public RestArgs(EndpointId character, RestType rest)
        {
            Character = character;
            Rest      = rest;
        }
    }
}