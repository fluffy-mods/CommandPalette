// HistoryList.cs
// Copyright Karel Kroeze, 2020-2020

using System.Collections;
using System.Collections.Generic;

namespace CommandPalette
{
    public class HistoryList<T>: IEnumerable<T>
    {
        private List<T> data = new List<T>();
        private int size;

        public HistoryList( int size )
        {
            this.size = size;
        }

        public void Add( T datum )
        {
            if ( data.Contains( datum ) )
                data.Remove( datum );
            data.Insert( 0, datum );
            while ( data.Count > size )
                data.RemoveAt( size );
        }

        public IEnumerator<T> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}