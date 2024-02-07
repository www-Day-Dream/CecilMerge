namespace CecilMerge.Puzzle
{
    internal interface IResolvable<in TResolver>
    {
        bool IsResolved { get; }

        void Resolve(TResolver resolveFrom);
    }
}