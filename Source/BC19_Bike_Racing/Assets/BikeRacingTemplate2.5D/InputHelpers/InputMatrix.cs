using UnityEngine;
using System.Collections.Generic;
using System;

namespace Kamgam.InputHelpers
{
    /// <summary>
    /// The goal of this whole thing is to manage which logic is currently receiving input.
    /// In theory it should always only be one (the currently active dialog) but in practice
    /// you will have things like Tooltips, Headers, Debug overlays, etc. which all require
    /// none exclusive input. In order to keep the chaos in check this class uses a 3d matrix
    /// (thus the name).<br />
    /// <br />
    /// Think of it like physical block which are hit by rain from top to bottom. Only those
    /// which get wet will receive input.<br />
    /// <br />
    /// Matrix Layout (3D meaning X,Y,Z):<br />
    ///  --> X: Order of execution (Right now it is only the "order of checking" not execution.)<br />
    ///  |<br />
    ///  v<br />
    ///  Y: Input Priority (Whatever object is on top will get the input and block all others in
    ///  the same column.<br />
    ///     One object can occupy multiple columns.).<br />
    ///  Z: Stack (see description below).<br />
    ///  <br />
    /// Example<br />:
    /// public enum UIStack { Debug, Loading, MenuLeft, MenuRight, ModalDialog }<br />
    /// <br />
    /// public class InputMatrix : Kamgam.InputMatrix.InputMatrix<UIStack><br />
    /// { }<br />
    /// <br />
    /// InputMatrix.Instance.SetMatrix(
    ///         new UIStack?[,]
    ///         {
    ///             { UIStack.Debug    , UIStack.Debug         },
    ///             { UIStack.Loading  , UIStack.Loading       },
    ///             { null             , UIStack.ModalDialog   },
    ///             { UIStack.MenuLeft , UIStack.MenuRight     },
    ///         }
    ///     );<br />
    ///     <br />
    /// Do not mistake this matrix for your ui layout!
    /// It's more like a "who is blocking whom from input" matrix.
    /// It will coincide mostly with the layers of your ui (what's on top) but that's not what
    /// it was made for. It's about logic, not the visuals.<br />
    /// <br />
    /// If an object is added to UiStack.Debug via "InputMatrix.Push(UIStack.Debug, debugLogic);"
    /// then for as long as it is not popped it will block all other objects from getting input
    /// because it is on top of all the others (see first row). The same is true for
    /// Loading. However, Loading can be blocked by Debug (if Debug is not null) because it is
    /// lower in the columns (see second row).<br />
    /// <br />
    /// On the other hand, if neither Debug nor Loading is set then MenuLeft and ModalDialog
    /// can receive input at the same time (in parallel).<br />
    /// <br />
    /// It may be that you have multiple ModalDialogs to show on top of each other. Each UIStack.* cell
    /// in the matrix is actually a link to a stack which can hold many items (Z). In order to add a new
    /// modal dialog just push it to the same stack. The important thing is that in that case only
    /// the TOP entry in the stack will receive input. You will have to pop them in the same order
    /// as you've pushed them.<br />
    /// <br />
    /// One stack can occupy multiple cells in the matrix (they will all refer to the same stack). 
    /// In the example above there is only one Debug stack, not two (the same goes for Loading).
    /// If a stack has and object pushed to it, it will block all input below in all columns.
    /// Each stack will only ever have one and exactly one active object (the one on top).<br />
    /// <br />
    /// The Matrix itself does not actually handle any input. It simply notifies IInputMatrixReceivers.
    /// </summary>
    public partial class InputMatrix<T> where T : struct, System.IConvertible
    {
        #region Singleton
        static InputMatrix<T> instance;

        public static InputMatrix<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new InputMatrix<T>();
                }
                return instance;
            }
        }
        #endregion

        public bool LogEnabled = false;

        // Init with a reasonable size to minimize runtime allocations.
        Dictionary<T, Stack> stacks = new Dictionary<T, Stack>(20);

        T?[,] matrix;

        // Init list with a certain size to minimize runtime allocations.
        List<Component> activeListeners = new List<Component>(20);

        // List to store results temporarily.
        private List<StackEntry> tmpResults = new List<StackEntry>(20);

        public InputMatrix()
        {
            if (!typeof(T).IsEnum)
                throw new System.ArgumentException("InputMatrix.Push(): T must be an enumerated type!");
        }

        public bool HasMatrix()
        {
            return matrix != null;
        }

        /// <summary>
        /// Set the input matrix. Use Nullable integers to fill it.
        /// Be aware that this will remove all currently registered listeners and stacks.
        /// </summary>
        /// <typeparam name="T">Has to be castable to int.</typeparam>
        /// <param name="matrix"></param>
        public void SetMatrix(T?[,] matrix)
        {
            activeListeners.Clear();

            // return old stack to pool
            if (stacks.Count > 0)
            {
                foreach (var kv in stacks)
                {
                    returnStack(kv.Value);
                }
                stacks.Clear();
            }

            this.matrix = matrix;

            if (this.matrix.GetLength(0) > 0 && this.matrix.GetLength(1) > 0)
            {
                T? stack;
                for (int row = 0; row < this.matrix.GetLength(0); row++)
                {
                    for (int column = 0; column < this.matrix.GetLength(1); column++)
                    {
                        stack = this.matrix[row, column];
                        if (stack.HasValue)
                        {
                            if (stacks.ContainsKey(stack.Value) == false)
                            {
                                stacks.Add(stack.Value, getNewStack());
                            }
                        }
                    }
                }
            }
        }

        public void Add(T stackId, Component listener)
        {
            Push(stackId, listener);
        }

        /// <summary>
        /// Add an object to the list of listeners for a specific stack.
        /// </summary>
        /// <param name="stackId"></param>
        /// <param name="listener"></param>
        /// <param name="allowParallelInput">Set to true if this object should not obfuscate (block) objects below it from receiving input</param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public void Push(T stackId, Component listener, bool allowParallelInput = false)
        {
            if (listener == null)
                throw new System.ArgumentException("InputMatrix.Push(): listener is null!");

            if (stacks != null)
            {
                if (stacks.ContainsKey(stackId))
                {
                    if (stacks[stackId].Count == 0 || !containsListener(stacks[stackId].Peek(tmpResults), listener))
                    {
                        if (LogEnabled)
                            Debug.Log("IM: Pushing " + (allowParallelInput ? "parallel" : "") + " listener '" + listener.ToString() + "' to stack '" + stackId.ToString() + "' at " + Time.realtimeSinceStartup + ".");

                        var newEntry = getNewStackEntry(listener, allowParallelInput);
                        stacks[stackId].Push(newEntry);
                        updateListeners();
                    }
                    else
                    {
                        if (LogEnabled)
                            Debug.Log("The listener (" + listener.ToString() + ") is already at the top of the stack " + stackId.ToString() + ", therefore this push() request is ignored.");
                    }
                }
                else
                {
                    throw new Exception("InputMatrix.Push(): Stack " + stackId.ToString() + " is not in the matrix. Update the matrix first.");
                }
            }
        }

        bool containsListener(List<StackEntry> list, Component listener)
        {
            foreach (var entry in list)
            {
                if (entry.Listener == listener)
                    return true;
            }
            return false;
        }

        StackEntry getStackEntry(List<StackEntry> list, Component listener)
        {
            foreach (var entry in list)
            {
                if (entry.Listener == listener)
                    return entry;
            }
            return null;
        }

        public void Remove(T stackId, Component listener)
        {
            Pop(stackId, listener);
        }

        /// <summary>
        /// Remove an object from a stack.
        /// </summary>
        /// <param name="stackId"></param>
        /// <param name="listener"></param>
        public void Pop(T stackId, Component listener)
        {
            if (listener == null)
                throw new System.ArgumentException("InputMatrix.Pop(): listener is null!");

            if (stacks != null)
            {
                if (stacks.ContainsKey(stackId) && stacks[stackId].Count > 0 && containsListener(stacks[stackId].Peek(tmpResults), listener))
                {
                    var entry = getStackEntry(tmpResults, listener);
                    if (entry.AllowParallelInput)
                    {
                        stacks[stackId].Pop(listener);
                        if (LogEnabled)
                            Debug.Log("InputMatrix.Pop(): Popping parallel listener '" + listener.ToString() + "' from stack '" + stackId.ToString() + "' at " + Time.realtimeSinceStartup + ".");
                    }
                    else
                    {
                        stacks[stackId].PopAllAbove(listener);
                        if (LogEnabled)
                            Debug.Log("InputMatrix.Pop(): Popping all above listener '" + listener.ToString() + "' from stack '" + stackId.ToString() + "' at " + Time.realtimeSinceStartup + ".");
                    }
                    updateListeners();
                }
                else
                {
                    if (LogEnabled)
                    {
                        Debug.Log("InputMatrix.Pop(): Listener not on top of given stack ( stack: " + stackId.ToString() + ", listener: " + listener.ToString() + "). Current Top is: " + ((stacks[stackId].Count > 0) ? stacks[stackId].PeekToString() : "empty"));
                    }
                }
            }
            else
            {
                throw new Exception("InputMatrix.Pop(): Stacks are null (stack: " + stackId.ToString() + ", listener: " + listener.ToString() + ").");
            }
        }

        /// <summary>
        /// Gets the current top objects and stores them in the given outList.<br />
        /// Results in an empty list if the stack is empty.<br />
        /// NOTICE: outList is cleared before it is filled.
        /// </summary>
        public void Peek(T stack, List<object> outList)
        {
            outList.Clear();

            if (stacks != null)
            {
                Debug.LogError("InputMatrix.Peek(): Stacks are empty!");
                return;
            }

            if (!stacks.ContainsKey(stack))
            {
                Debug.LogError("InputMatrix.Peek(): No such stack found (" + stack.ToString() + ").");
                return;
            }

            if (stacks[stack].Count == 0)
                return;

            stacks[stack].Peek(tmpResults);
            foreach (var entry in tmpResults)
            {
                outList.Add(entry.Listener);
            }
        }

        /// <summary>
        /// This searches for the obj in the currently active objects. Remember: the objects which the component is checked against are the exact same objects you added via Push().<br />
        /// NOTICE: It will return false for children or parents ob active objects as it does NOT search the hierarchy. Use the non-raw methods for that.<br />
        /// It only returns true if the very same component which was used in Push() is given.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool ShouldReceiveInputRaw(Component component)
        {
            return activeListeners.Contains(component);
        }

        /// <summary>
        /// Checks if the given Component is a child of any active object.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public bool ShouldReceiveInput(Component component)
        {
            return ShouldReceiveInput(component.transform);
        }

        /// <summary>
        /// Checks if the given GameObject is a child of any active object.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public bool ShouldReceiveInput(GameObject gameObject)
        {
            return ShouldReceiveInput(gameObject.transform);
        }

        /// <summary>
        /// Checks if the given Transform is a child of any active object.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public bool ShouldReceiveInput(Transform transform)
        {
            foreach (var listener in activeListeners)
            {
                if (listener == null || listener.gameObject == null)
                    continue;

                if (transform.gameObject == listener)
                    return true;

                if (transform.IsChildOf(listener.transform))
                    return true;
            }

            return false;
        }

        protected List<Component> tmpOldActiveListeners = new List<Component>(10);


        protected void updateListeners()
        {
            tmpOldActiveListeners.Clear();
            foreach (var listener in activeListeners)
            {
                tmpOldActiveListeners.Add(listener);
            }

            // update active listeners
            activeListeners.Clear();
            fillActiveListenersList(activeListeners);

            // notify removed listeners
            foreach (var oldListener in tmpOldActiveListeners)
            {
                if (!activeListeners.Contains(oldListener))
                {
                    var oldReceiver = oldListener as IInputMatrixReceiver;
                    if (oldReceiver != null)
                        oldReceiver.OnDeactivatedInMatrix();
                }
            }

            // notify newly added listeners
            foreach (var newListener in activeListeners)
            {
                if (!tmpOldActiveListeners.Contains(newListener))
                {
                    var newReceiver = newListener as IInputMatrixReceiver;
                    if (newReceiver != null)
                        newReceiver.OnActivatedInMatrix();
                }
            }

            if (LogEnabled)
            {
                LogActiveListeners();
            }

            tmpOldActiveListeners.Clear();
        }

        /// <summary>
        /// Clears and then fills outActiveListeners with the currently active listeners.
        /// This is where the actual logic of the Input Matrix happens.
        /// </summary>
        /// <param name="outActiveListeners"></param>
        protected void fillActiveListenersList(List<Component> outActiveListeners)
        {
            outActiveListeners.Clear();

            if (matrix.GetLength(0) > 0 && matrix.GetLength(1) > 0)
            {
                // Check each cell if the stack within it contains listeners.
                T? stackId;
                for (int row = 0; row < matrix.GetLength(0); row++)
                {
                    for (int column = 0; column < matrix.GetLength(1); column++)
                    {
                        stackId = matrix[row, column];
                        if (!stackId.HasValue || stacks[stackId.Value].Count == 0)
                            continue;

                        // If yes then add then to the active listeners list (if not yet added).
                        stacks[stackId.Value].Peek(tmpResults);
                        if (tmpResults.Count > 0)
                        {
                            foreach (var entry in tmpResults)
                            {
                                if (!outActiveListeners.Contains(entry.Listener))
                                    outActiveListeners.Add(entry.Listener);
                            }

                            // top listeners found, skip the rest
                            break;
                        }
                    }
                }
            }
        }

        public void LogActiveListeners()
        {
            if (activeListeners.Count > 0)
            {
                Debug.Log("IM: Active listeners at " + Time.realtimeSinceStartup + " :");
                foreach (var listener in activeListeners)
                {
                    T? stack = getStackIdOfListener(listener);
                    var entry = getStackEntryOfListener(listener);
                    Debug.Log("  - Stack: " + (stack.HasValue ? stack.Value.ToString() : "null") + ", "+ (entry.AllowParallelInput ? "Parallel " : " ") + "Listener: " + listener.ToString() + ")");
                }
            }
            else
            {
                Debug.Log("IM: no active listeners at the moment.");
            }
        }

        protected T? getStackIdOfListener(Component listener)
        {
            if (matrix.GetLength(0) > 0 && matrix.GetLength(1) > 0)
            {
                T? stackId;
                for (int row = 0; row < matrix.GetLength(0); row++)
                {
                    for (int column = 0; column < matrix.GetLength(1); column++)
                    {
                        stackId = matrix[row, column];
                        if (!stackId.HasValue || stacks[stackId.Value].Count == 0)
                            continue;

                        stacks[stackId.Value].Peek(tmpResults);
                        if (containsListener(tmpResults, listener))
                        {
                            return stackId.Value;
                        }
                    }
                }
            }

            return null;
        }

        protected StackEntry getStackEntryOfListener(Component listener)
        {
            foreach (var kv in stacks)
            {
                kv.Value.GetActiveEntries(tmpResults);
                foreach (var entry in tmpResults)
                {
                    if (entry.Listener == listener)
                        return entry;
                }
            }

            return null;
        }
    }
}
