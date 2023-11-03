using System.Collections ;

namespace HcBimUtils.MoreLinq
{
   static partial class MoreEnumerable
   {
      /// <summary>
      /// Flattens a sequence containing arbitrarily-nested sequences.
      /// </summary>
      /// <param name="source">The sequence that will be flattened.</param>
      /// <returns>
      /// A sequence that contains the elements of <paramref name="source"/>
      /// and all nested sequences (except strings).
      /// </returns>
      /// <exception cref="System.ArgumentNullException"><paramref name="source"/> is null.</exception>

      public static IEnumerable<object> Flatten(this IEnumerable source) =>
          Flatten(source, obj => !(obj is string));

      /// <summary>
      /// Flattens a sequence containing arbitrarily-nested sequences. An
      /// additional parameter specifies a predicate function used to
      /// determine whether a nested <see cref="IEnumerable"/> should be
      /// flattened or not.
      /// </summary>
      /// <param name="source">The sequence that will be flattened.</param>
      /// <param name="predicate">
      /// A function that receives each element that implements
      /// <see cref="IEnumerable"/> and indicates if its elements should be
      /// recursively flattened into the resulting sequence.
      /// </param>
      /// <returns>
      /// A sequence that contains the elements of <paramref name="source"/>
      /// and all nested sequences for which the predicate function
      /// returned <c>true</c>.
      /// </returns>
      /// <exception cref="System.ArgumentNullException">
      /// <paramref name="source"/> is <c>null</c>.</exception>
      /// <exception cref="System.ArgumentNullException">
      /// <paramref name="predicate"/> is <c>null</c>.</exception>

      public static IEnumerable<object> Flatten(this IEnumerable source, Func<IEnumerable, bool> predicate)
      {
         if (predicate == null) throw new ArgumentNullException(nameof(predicate));

         return Flatten(source, obj => obj is IEnumerable inner && predicate(inner) ? inner : null);
      }

      /// <summary>
      /// Flattens a sequence containing arbitrarily-nested sequences. An
      /// additional parameter specifies a function that projects an inner
      /// sequence via a property of an object.
      /// </summary>
      /// <param name="source">The sequence that will be flattened.</param>
      /// <param name="selector">
      /// A function that receives each element of the sequence as an object
      /// and projects an inner sequence to be flattened. If the function
      /// returns <c>null</c> then the object argument is considered a leaf
      /// of the flattening process.
      /// </param>
      /// <returns>
      /// A sequence that contains the elements of <paramref name="source"/>
      /// and all nested sequences projected via the
      /// <paramref name="selector"/> function.
      /// </returns>
      /// <exception cref="System.ArgumentNullException">
      /// <paramref name="source"/> is <c>null</c>.</exception>
      /// <exception cref="System.ArgumentNullException">
      /// <paramref name="selector"/> is <c>null</c>.</exception>

      public static IEnumerable<object> Flatten(this IEnumerable source, Func<object, IEnumerable> selector)
      {
         if (source == null) throw new ArgumentNullException(nameof(source));
         if (selector == null) throw new ArgumentNullException(nameof(selector));

         return _(); IEnumerable<object> _()
         {
            var e = source.GetEnumerator();
            var stack = new Stack<IEnumerator>();

            stack.Push(e);

            try
            {
               while (stack.Any())
               {
                  e = stack.Pop();

               reloop:

                  while (e.MoveNext())
                  {
                     if (selector(e.Current) is IEnumerable inner)
                     {
                        stack.Push(e);
                        e = inner.GetEnumerator();
                        goto reloop;
                     }
                     else
                     {
                        yield return e.Current;
                     }
                  }

                   (e as IDisposable)?.Dispose();
                  e = null;
               }
            }
            finally
            {
               (e as IDisposable)?.Dispose();
               foreach (var se in stack)
                  (se as IDisposable)?.Dispose();
            }
         }
      }
   }
}