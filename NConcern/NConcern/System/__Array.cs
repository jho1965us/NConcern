namespace System
{
    public static class __Array
    {
        static public T[] Subarray<T>(this T[] array, int startIndex, int count)
        {
            if (startIndex == 0 && count == array.Length) return array;
            var _values = new T[count];
            Array.Copy(array, startIndex, _values, 0, count);
            return _values;
        }
        static public T[] Subarray<T>(this T[] array, int startIndex)
        {
            var _count = array.Length - startIndex;
            if (startIndex == 0 && _count == array.Length) return array;
            var _values = new T[_count];
            Array.Copy(array, startIndex, _values, 0, _count);
            return _values;
        }
    }
}