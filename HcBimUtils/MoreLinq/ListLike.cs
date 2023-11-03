namespace HcBimUtils.MoreLinq
{
   /// <summary>
   /// Represents an list-like (indexable) data structure.
   /// </summary>

   internal interface IListLike<out T>
   {
      int Count { get; }
      T this[int index] { get; }
   }

   internal static class ListLike
   {
      public static IListLike<T> ToListLike<T>(this IEnumerable<T> source)
          => source.TryAsListLike() ?? new List<T>(source.ToList());

      public static IListLike<T> TryAsListLike<T>(this IEnumerable<T> source)
          => source is null ? throw new ArgumentNullException(nameof(source))
           : source is IList<T> list ? new List<T>(list)
           : source is IReadOnlyList<T> readOnlyList ? new ReadOnlyList<T>(readOnlyList)
           : (IListLike<T>)null;

      private sealed class List<T> : IListLike<T>
      {
         private readonly IList<T> _list;

         public List(IList<T> list) => _list = list ?? throw new ArgumentNullException(nameof(list));

         public int Count => _list.Count;
         public T this[int index] => _list[index];
      }

      private sealed class ReadOnlyList<T> : IListLike<T>
      {
         private readonly IReadOnlyList<T> _list;

         public ReadOnlyList(IReadOnlyList<T> list) => _list = list ?? throw new ArgumentNullException(nameof(list));

         public int Count => _list.Count;
         public T this[int index] => _list[index];
      }
   }
}