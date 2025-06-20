﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WASender
{
    public static class StaticMethods
    {
        public readonly static string media = "media";
        public readonly static string location = "location";
        public readonly static string contact = "contact";
        public readonly static string audio = "audio";
        public readonly static string sticker = "sticker";
        public readonly static string videoNote = "videoNote";


        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
