using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kamgam.InputHelpers
{
    public static class EventTriggerUtils
    {
        /// <summary>
        /// Adds EventTriggers to all Buttons and then adds a callback for Sumit and PointerUp to all EventTriggers.<br />
        /// This can be called multiple times without causing duplicate callback (for cases when objects are added dynamically).
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="callback"></param>
        /// <param name="includeInactive"></param>
        public static void AddOnClickTriggersToAllChildren(GameObject obj, UnityEngine.Events.UnityAction<BaseEventData> callback, bool includeInactive = true)
        {
            var eventTriggers = obj.GetComponentsInChildren<EventTrigger>(includeInactive);
            foreach (var trigger in eventTriggers)
            {
                AddCallbackToEventTrigger(trigger, EventTriggerType.Submit, callback);
                AddCallbackToEventTrigger(trigger, EventTriggerType.PointerUp, callback);
            }

            var buttons = obj.GetComponentsInChildren<Button>(includeInactive);
            foreach (var btn in buttons)
            {
                AddEventTrigger(btn.gameObject, EventTriggerType.Submit, callback);
                AddEventTrigger(btn.gameObject, EventTriggerType.PointerUp, callback);
            }
        }

        /// <summary>
        /// Adds an EventTrigger component if needed and then adds the callback to it.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        public static void AddEventTrigger(GameObject obj, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            var eventTrigger = obj.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = obj.AddComponent<EventTrigger>();

            AddCallbackToEventTrigger(eventTrigger, type, callback);
        }

        /// <summary>
        /// Adds a callback to the eventTrigger.
        /// </summary>
        /// <param name="eventTrigger"></param>
        /// <param name="type"></param>
        /// <param name="callback"></param>
        public static void AddCallbackToEventTrigger(EventTrigger eventTrigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
        {
            bool appendedToExisting = false;
            var entries = eventTrigger.triggers;
            foreach (var entry in entries)
            {
                if(entry.eventID == type)
                {
                    entry.callback.RemoveListener(callback);
                    entry.callback.AddListener(callback);
                    appendedToExisting = true;
                }
            }

            if (!appendedToExisting)
            {
                EventTrigger.Entry newEntry = new EventTrigger.Entry();
                newEntry.eventID = type;
                newEntry.callback.AddListener(callback);
                eventTrigger.triggers.Add(newEntry);
            }
        }
    }
}
