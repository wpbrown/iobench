// Copyright 2014 ExxonMobil Technical Computing Company
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExxonMobil.Shared
{
    public static class Enumerable
    {
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
			foreach(T item in collection)  
				action(item);

			return collection;
		}

		public static IEnumerable<T> ForEach<T>(this IList<T> collection, Action<T> action)
		{
			for (int i = 0; i < collection.Count; ++i)
				action(collection[i]);

			return collection;
		}

		public static IEnumerable<T> ForEach<T>(this IList<T> collection, Func<T, T> func)
		{
			for (int i = 0; i < collection.Count; ++i)
				collection[i] = func(collection[i]);

			return collection;
		}

		public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> func)
		{
			if (first == null)
				throw new ArgumentNullException("first");

			if (second == null)
				throw new ArgumentNullException("second");

			return ZipInternal(first, second, func);
		}

		private static IEnumerable<TResult> ZipInternal<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> func)
		{
			var ie1 = first.GetEnumerator();
			var ie2 = second.GetEnumerator();
			while (ie1.MoveNext() && ie2.MoveNext())
				yield return func(ie1.Current, ie2.Current);
		}

		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> source, Func<T, object> check)
		{
			return source.Distinct(new GenericComparer<T>(check));
		}

		public static Dictionary<U, T> Index<T, U>(this IEnumerable<T> source, Func<T, U> selectIndexer)
		{
			var dictionary = new Dictionary<U, T>();
         foreach (T item in source)
         {
            var indexer = selectIndexer(item);
            if(indexer != null)
               dictionary.Add(selectIndexer(item), item);
         }
			return dictionary;
		}
    }
}
