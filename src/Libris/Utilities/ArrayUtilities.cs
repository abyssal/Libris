﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Libris.Utilities
{
    public static class ArrayUtilities
    {
        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] ret = new byte[arrays.Sum(x => x.Length)];
            int offset = 0;
            foreach (var data in arrays)
            {
                Buffer.BlockCopy(data, 0, ret, offset, data.Length);
                offset += data.Length;
            }
            return ret;
        }

        public static byte[] Add(this byte[] set, byte value)
        {
            var newArray = new byte[set.Length + 1];
            Buffer.BlockCopy(set, 0, newArray, 0, set.Length);
            newArray[set.Length] = value;
            return newArray;
        }
    }
}