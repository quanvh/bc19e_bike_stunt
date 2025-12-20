using System.Collections.Generic;
using UnityEngine;


namespace Kamgam.BikeAndCharacter25D.Helpers
{
    public class FloatAverageQueue
    {
        public int Capacity { get; private set; }
        public int Count { get; private set; } = 0;
        public float AverageValue { get; private set; } = 0;
        protected float[] values;
        protected int lastIndex = -1;
        protected int nextIndex = 0;

        public FloatAverageQueue(int capacity)
        {
            Capacity = capacity;
            values = new float[capacity];
            Count = 0;
        }

        /// <summary>
        /// Enqueues and calcs the average.
        /// </summary>
        /// <param name="value"></param>
        public void Enqueue(float? value)
        {
            if (value.HasValue)
            {
                Enqueue(value.Value);
            }
        }

        /// <summary>
        /// Enqueues
        /// </summary>
        /// <param name="value"></param>
        public void Enqueue(float value, bool updateAverage = true)
        {
            values[nextIndex] = value;
            lastIndex = nextIndex;
            nextIndex = (nextIndex + 1) % Capacity;

            if (Count < Capacity)
            {
                Count++;
            }

            if(updateAverage)
            {
                UpdateAverage();
            }
        }

        public void Dequeue()
        {
            if (Count > 0)
            {
                Count--;
            }
        }

        public void UpdateAverage()
        {
            UpdateAverage(Count);
        }

        public void UpdateAverage(int numOfEntriesToAverage)
        {
            if(numOfEntriesToAverage == 0)
            {
                AverageValue = 0;
                return;
            }

            numOfEntriesToAverage = Mathf.Min(Count, numOfEntriesToAverage);

            float sum = 0;
            int idx;
            for (int i = 0; i < numOfEntriesToAverage; i++)
            {
                idx = ((nextIndex - numOfEntriesToAverage + i) % Capacity + Capacity) % Capacity; // mod(..) inlined
                sum += values[idx];
            }
            AverageValue = sum / numOfEntriesToAverage;
        }

        /// <summary>
        /// Transforms negative k to a positive index.
        /// </summary>
        /// <param name="k"></param>
        /// <param name="n"></param>
        /// <returns>Results are positive, even for negative k.</returns>
        protected int mod(int k, int n)
        {
            return (k % n + n) % n;
        }

        public float Average()
        {
            return AverageValue;
        }

        public float Last()
        {
            return values[lastIndex];
        }

        public void TakeLastNonAlloc( int N, ref List<float> result)
        {
            result.Clear();
            N = Mathf.Min(N, Count);
            int idx;
            for (int i = 0; i < N; i++)
            {
                idx = mod(nextIndex - N + i, Capacity);
                result.Add(values[idx]);
            }
        }

        public void Clear()
        {
            Count = 0;
            lastIndex = -1;
            nextIndex = 0;
            AverageValue = 0;
        }
    }
}
