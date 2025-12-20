using UnityEngine;
using System.Collections.Generic;

namespace Kamgam.InputHelpers
{
    public partial class InputMatrix<T>
    {
        public class Stack
        {
            public bool LoggingEnabled;

            List<StackEntry> entries = new List<StackEntry>(10);

            public int Count => entries.Count;

            public void Clear(System.Action<StackEntry> returnStackEntry)
            {
                if (entries.Count == 0)
                    return;

                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    returnStackEntry(entries[i]);
                    entries.RemoveAt(i); 
                }
            }

            public List<StackEntry> Peek(List<StackEntry> outActiveEntries)
            {
                return GetActiveEntries(outActiveEntries);
            }

            public string PeekToString()
            {
                string result = "";

                List<StackEntry> list = new List<StackEntry>();
                var entries = GetActiveEntries(list);

                foreach (var entry in entries)
                {
                    var obj = entry.Listener as UnityEngine.Object;
                    if(obj )
                        result += obj.name + ", ";
                    else
                        result += entry.Listener.ToString() + ", ";
                }

                return result;
            }

            public List<StackEntry> GetActiveEntries(List<StackEntry> outActiveEntries)
            {
                outActiveEntries.Clear();

                // go down from back until the first entry with AllowParallelInput = false is found.
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    outActiveEntries.Add(entries[i]);
                    if (!entries[i].AllowParallelInput)
                        break;
                }

                return outActiveEntries;
            }

            public List<StackEntry> GetFirstInactiveEntries(List<StackEntry> outInactiveEntries)
            {
                outInactiveEntries.Clear();

                // go down from back until the first entry with AllowParallelInput = false is found.
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    if (!entries[i].AllowParallelInput)
                    {
                        // Do it again but now start from the last found active entry.
                        // These are the first inactive results.
                        if (i - 1 >= 0)
                        {
                            for (int j = i - 1; j >= 0; j--)
                            {
                                outInactiveEntries.Add(entries[j]);
                                if (!entries[j].AllowParallelInput)
                                    break;
                            }
                        }
                        break;
                    }
                }

                return outInactiveEntries;
            }

            public void Push(StackEntry entry)
            {
                entries.Add(entry);
            }

            public void Pop(Component listener)
            {
                // go forward from back until the entry is found and remove it.
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    if (entries[i].Listener == listener)
                    {
                        if (LoggingEnabled)
                        {
                            Debug.Log("  IM-Stack: Popping listener '" + entries[i].ToString() + "' at " + Time.realtimeSinceStartup + ".");
                        }
                        entries.RemoveAt(i);
                        break;
                    }

                    // Don't allow to pop below the first entry with AllowParallelInput = false.
                    if (!entries[i].AllowParallelInput)
                        break;
                }
            }

            /// <summary>
            /// Removes all entries above the given entry, including the given entry.
            /// </summary>
            /// <param name="entry"></param>
            public void PopAllAbove(Component listener)
            {
                // go forward from back until the entry is found and remove all after it.
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    if (entries[i].Listener == listener)
                    {
                        if(LoggingEnabled)
                        {
                            for (int j = i; j < entries.Count; j++)
                            {
                                Debug.Log("  IM-Stack: Popping listener '" + entries[j].ToString() + "' at " + Time.realtimeSinceStartup + ".");
                            }
                        }
                        entries.RemoveRange(i, entries.Count - i);
                        break;
                    }
                    // Don't allow to pop below the first entry with AllowParallelInput = false.
                    if (!entries[i].AllowParallelInput)
                        break;
                }
            }
        }

        public class StackEntry
        {
            public Component Listener;
            public bool AllowParallelInput;

            public StackEntry(Component listener, bool allowParallelInput)
            {
                Listener = listener;
                AllowParallelInput = allowParallelInput;
            }

            public void Clear()
            {
                Listener = null;
                AllowParallelInput = false;
            }
        }
        

        // Stack pool for reusing Stack objects.
        // These pool shenanigans are done to reduce GC at runtime.

        List<Stack> stackPool = new List<Stack>(10);

        Stack getNewStack()
        {
            // create new if needed
            if (stackPool.Count == 0)
                stackPool.Add(new Stack());

            // init, remove from pool and return
            var stack = stackPool[stackPool.Count - 1];
            stack.LoggingEnabled = LogEnabled;
            stackPool.RemoveAt(stackPool.Count - 1);
            return stack;
        }

        void returnStack(Stack stack)
        {
            stack.Clear(returnStackEntry);
            stackPool.Add(stack);

            if (LogEnabled)
                Debug.Log("IM: stack pool sice: " + stackPool.Count);
        }


        // StackEntry pool for reusing StackEntry objects.
        List<StackEntry> stackEntryPool = new List<StackEntry>(10);

        StackEntry getNewStackEntry(Component listener, bool allowParallelInput)
        {
            // create new if needed
            if (stackEntryPool.Count == 0)
                stackEntryPool.Add(new StackEntry(listener, allowParallelInput));

            // remove from pool and return
            var entry = stackEntryPool[stackEntryPool.Count - 1];
            stackEntryPool.RemoveAt(stackEntryPool.Count - 1);
            return entry;
        }

        void returnStackEntry(StackEntry entry)
        {
            entry.Clear();
            stackEntryPool.Add(entry);

            if (LogEnabled)
                Debug.Log("IM: stack entry pool sice: " + stackEntryPool.Count);
        }
    }
}
