﻿// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers
{

    /// <summary>
    /// Extension methods for generic lists.
    /// </summary>
    public static class ListExtensions
    {

        /// <summary>
        /// Adds an item to a list in sorted order. The list items must implement
        /// IComparable.
        /// </summary>
        /// <param name="this">This list.</param>
        /// <param name="item">Item to add.</param>
        /// <typeparam name="T">The list type.</typeparam>
        public static void AddSorted<T>(this List<T> @this, T item) where T : IComparable<T>
        {
            if (@this.Count == 0)
            {
                @this.Add(item);
            }
            else if (@this[@this.Count - 1].CompareTo(item) <= 0)
            {
                @this.Add(item);
            }
            else if (@this[0].CompareTo(item) >= 0)
            {
                @this.Insert(0, item);
            }
            else
            {
                int index = @this.BinarySearch(item);
                if (index < 0)
                {
                    index = ~index;
                }
                @this.Insert(index, item);
            }
        }

        /// <summary>
        /// Fisher-Yates shuffle.
        /// </summary>
        public static void Shuffle<T>(this List<T> @this)
        {
            if (@this == null || @this.Count < 2) return;
            for (int i = 0; i < @this.Count - 1; i++)
            {
                int j = UnityEngine.Random.Range(i, @this.Count);
                T tmp = @this[i];
                @this[i] = @this[j];
                @this[j] = tmp;
            }
        }

        /// <summary>
        /// Returns a random element from a list.
        /// </summary>
        public static T GetRandomElement<T>(this List<T> @this)
        {
            if (@this == null) return default(T);
            return @this[UnityEngine.Random.Range(0, @this.Count)];
        }

    }

}
