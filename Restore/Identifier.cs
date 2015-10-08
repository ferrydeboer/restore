namespace Restore
{
    /// <summary>
    /// Object identifier abstraction to make <see cref="IDataEndpoint{T}"/> interface and implementation agnostic of
    /// the actual identifiers.
    /// </summary>
    /// <remarks>I think using IEquatable interface works perfectly fine here.</remarks>
    public abstract class Identifier
    {
        public static implicit operator Identifier(int id)
        {
            return new IntIdentifier(id);
        }
    }
}