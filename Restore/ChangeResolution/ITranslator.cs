namespace Restore.ChangeResolution
{
    /// <summary>
    /// A translator is a component capable of transforming one instance into another.
    /// </summary>
    /// <typeparam name="T1">The first type two which a the translation can be applied.</typeparam>
    /// <typeparam name="T2">The second type two which a the translation can be applied.</typeparam>
    public interface ITranslator<T1, T2>
    {
        void TranslateForward(T1 source, ref T2 target);
        void TranslateBackward(T2 source, ref T1 target);
    }
}
