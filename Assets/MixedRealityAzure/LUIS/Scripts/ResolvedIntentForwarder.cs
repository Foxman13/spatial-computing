﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Cognitive.LUIS;
using UnityEngine.EventSystems;

namespace Microsoft.MR.LUIS
{
    /// <summary>
    /// An <see cref="IIntentHandler"/> that routes supported intents to resolved scene entities.
    /// </summary>
    public class ResolvedIntentForwarder : IIntentHandler
    {
        #region Member Variables
        private List<string> excludedIntents = new List<string>();
        private List<string> includedIntents = new List<string>();
        private IntentEventData intentEventData;
        #endregion // Member Variables

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool CanHandle(string intentName)
        {
            // Is it explicitly excluded?
            if (excludedIntents.Contains(intentName))
            {
                return false;
            }

            // Is it either wildcard or explicitly included?
            if ((includedIntents.Count == 0) || (includedIntents.Contains(intentName)))
            {
                return true;
            }

            // Not supported
            return false;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Handle(Intent intent, LuisMRResult result)
        {
            // Validate
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            if (result == null) throw new ArgumentNullException(nameof(result));
            
            // Make sure event data has been created
            if (intentEventData == null)
            {
                intentEventData = new IntentEventData(EventSystem.current);
            }

            // Update event data to current values
            intentEventData.Initialize(intent, result);

            // Get all resolved entities
            List<EntityMap> resolvedEntities = result.GetAllResolvedEntities();
            
            // Forward to all resolved entities
            foreach (EntityMap map in resolvedEntities)
            {
                // Execute on the game object
                if (SendToChildren)
                {
                    ExecuteEvents.ExecuteHierarchy(map.GameObject, intentEventData, OnIntentHandler);
                }
                else
                {
                    ExecuteEvents.Execute(map.GameObject, intentEventData, OnIntentHandler);
                }
            }
        }

        private static readonly ExecuteEvents.EventFunction<IIntentHandler> OnIntentHandler =
            delegate (IIntentHandler handler, BaseEventData eventData)
            {
                var casted = ExecuteEvents.ValidateEventData<IntentEventData>(eventData);
                handler.Handle(casted.Intent, casted.Result);
                casted.Use();
            };

        /// <summary>
        /// Gets the list of explicitly excluded intents.
        /// </summary>
        public List<string> ExcludedIntents => excludedIntents;

        /// <summary>
        /// Gets the list of explicitly included intents.
        /// </summary>
        /// <remarks>
        /// If no intents are explicitly included, all intents will be forwarded.
        /// </remarks>
        public List<string> IncludedIntents => includedIntents;

        /// <summary>
        /// Gets or set a value that indicates if events should be sent to child game objects.
        /// </summary>
        /// <value>
        /// <c>true</c> if if events should be sent to child game objects; otherwise <c>false</c>. The default is <c>false</c>.
        /// </value>
        public bool SendToChildren { get; set; }
    }
}
