using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ensage.Common;

namespace HuskarSharp.Features.Orbwalker
{
    public class MultiSleeper
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSleeper"/> class.
        /// </summary>
        public MultiSleeper()
        {
            this.LastSleepTickDictionary = new Dictionary<object, float>();
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the last sleep tick dictionary.
        /// </summary>
        public Dictionary<object, float> LastSleepTickDictionary { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The reset.
        /// </summary>
        /// <param name="id">
        ///     The id.
        /// </param>
        public void Reset(object id)
        {
            if (!this.LastSleepTickDictionary.ContainsKey(id))
            {
                return;
            }

            this.LastSleepTickDictionary[id] = 0;
        }

        /// <summary>
        /// The sleep.
        /// </summary>
        /// <param name="duration">
        /// The duration.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="extendCurrentSleep">
        /// The extend current sleep.
        /// </param>
        public void Sleep(float duration, object id, bool extendCurrentSleep = false)
        {
            if (!this.LastSleepTickDictionary.ContainsKey(id))
            {
                this.LastSleepTickDictionary.Add(id, Utils.TickCount + duration);
                return;
            }

            if (extendCurrentSleep && this.LastSleepTickDictionary[id] > Utils.TickCount)
            {
                this.LastSleepTickDictionary[id] += duration;
                return;
            }

            this.LastSleepTickDictionary[id] = Utils.TickCount + duration;
        }

        /// <summary>
        ///     The sleeping.
        /// </summary>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <returns>
        ///     The <see cref="bool" />.
        /// </returns>
        public bool Sleeping(object id)
        {
            float lastSleepTick;
            return this.LastSleepTickDictionary.TryGetValue(id, out lastSleepTick) && Utils.TickCount < lastSleepTick;
        }

        #endregion
    }
}
