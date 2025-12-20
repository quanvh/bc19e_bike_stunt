// Copyright (c) 2014-2018 Anthony Carapetis
// This software is licensed under the MIT license.
// See LICENSE for more details.
// C# Version: 2020 KAMGAM e.U.

using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Kamgam.CurveShorteningFlow
{
    public delegate T Neighbourhood<T>(int a);
    public delegate V LocalFunction<T, V>(T element, int index, Neighbourhood<T> neighbourhood);

    public class CircularList<T> : IEnumerable<T>
    {
        protected List<T> _data;
        public List<T> Data { get => _data; }

        public int Count { get => this._data.Count; }

        public CircularList()
        {
        }

        public CircularList(CircularList<T> list)
        { 
            this._data = list.Data;
        }

        public CircularList(List<T> data)
        {
            this._data = data;
        }

        protected void setData(List<T> data)
        {
            if(_data == null)
            {
                _data = data;
            }
            else
            {
                throw new System.Exception("CircularList: data is already set!");
            }
        }

        public T Get(int index)
        {
            return this._data[mod(index, this.Count)];
        }

        public void Set(int index, T value)
        {
            this._data[mod(index, this.Count)] = value;
        }

        public void Splice(int index, int count, params T[] values)
        {
            // easy cases
            if (count >= this.Count)
            {
                this._data = new List<T>();
                return;
            }

            if (index + count < this.Count)
            {
                this._data.Splice(index, count, values);
                return;
            }

            // wrap-around case
            int endLength = this.Count - index;
            int startLength = count - endLength;
            this._data.Splice(index, endLength, values.Take(endLength).ToArray());
            this._data.Splice(0, startLength, values.Take(endLength).ToArray());
        }

        public CircularList<T> Filter(LocalFunction<T,bool> fn)
        {
            var filteredData = new List<T>();
            for (int i = 0; i < _data.Count; i++)
            {
                if(fn(_data[i], i, this.neighbourhood(i)))
                {
                    filteredData.Add(_data[i]);
                }
            }

            return new CircularList<T>(filteredData);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public IEnumerable<Neighbourhood<T>> Neighbourhoods()
        {
            for (int i = 0; i < _data.Count; i++)
            {
                yield return this.neighbourhood(i);
            }
        }

        protected Neighbourhood<T> neighbourhood(int index)
        {
            return (int offset) => this.Get(index + offset);
        }

        public W Map<V, W>(LocalFunction<T, V> fn) where W : CircularList<V>, new()
        {
            var newList = new W();
            newList.setData(this._data.Select((x, i) => fn(x, i, this.neighbourhood(i))).ToList());
            return newList;
        }

        public void ForEach(LocalFunction<T,object> fn)
        {
            for (int i = 0; i < _data.Count; i++)
            {
                fn(_data[i], i, this.neighbourhood(i));
            }            
        }

        /// <summary>
        /// A sensible modulo:
        /// The unique integer congruent to k mod n in [0,..,n-1] 
        /// Transforms negative k to a positive index.
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns>Results are always positive, even for negative k.</returns>
        protected int mod(int k, int n)
        {
            return (k % n + n) % n;
        }
    }
}
