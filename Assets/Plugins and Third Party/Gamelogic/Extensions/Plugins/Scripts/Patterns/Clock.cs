﻿// Copyright Gamelogic (c) http://www.gamelogic.co.za

using System;
using Gamelogic.Extensions.Internal;
using UnityEngine;

namespace Gamelogic.Extensions
{
	/// <summary>
	/// Represents a clock that expires after a given time.
	/// </summary>
	/// <remarks>
	/// <para>To use this clock, instantiate it, call Reset with the right time value, and call Update it each frame.
	/// </para>
	/// <para>Any class that wants to be notified of events need to implement the IClockListener interface,
	/// and subscribe to events using the AddListener method. A listener can be removed with the RemoveListener
	/// event.
	/// </para>
	/// <para>Clocks can be paused independently of Time.timeScale using the Pause method (and started again using Unpause).
	/// </para>

	/// </remarks>
	[Version(1, 2)]
	public class Clock
	{
		#region Private Fields

		private float time;
		private ObservedValue<int> timeInSeconds;

		/// <summary>
		/// Occurs when the clock expired.
		/// </summary>
		public event Action OnClockExpired;

        /// <summary>
        /// Occurs when the seconds changed. Note that the seconds is the ceiling of the current time.
        /// </summary>
        public event Action OnSecondsChanged;
        
        /// <summary>
        /// OBSOLETE (Added by Nrealus) Occurs when the clock ticked (ie. Updates while not being paused or expired).
        /// </summary>
        //public event Action OnClockUpdated;

        #endregion

        #region  Properties		
        /// <summary>
        /// Gets a value indicating whether this clock is paused.
        /// </summary>
        /// <value><c>true</c> if this clock is paused; otherwise, <c>false</c>.</value>
        public bool IsPaused
		{
			get; private set;
		}

		/// <summary>
		/// Gets a value indicating whether this instance is done.
		/// </summary>
		/// <value><c>true</c> if this instance is done; otherwise, <c>false</c>.</value>
		public bool IsDone
		{
			get; private set;
		}

		/// <summary>
		/// The current time on this clock in seconds as a float.
		/// </summary>
		/// <value>The time.</value>
		public float Time
		{
			get
			{
				return time;
			}
		}

		/// <summary>
		/// Gets the time in seconds of this clock as an integer.
		/// </summary>
		/// <value>The time in seconds. The time in seconds is the ceiling of the time left.</value>
		public int TimeInSeconds
		{
			get
			{
				return timeInSeconds.Value;
			}
		}

		#endregion

		#region Constructors		
		/// <summary>
		/// Initializes a new instance of the <see cref="Clock"/> class.
		/// </summary>
		/// <remarks>The clock is initially paused, and set to expire immediately when unpaused.</remarks>
		public Clock()
		{
			IsPaused = true;
			timeInSeconds = new ObservedValue<int>(0);
			timeInSeconds.OnValueChange += NotifySecondsChanged;

			Reset(0);	
		}

		#endregion

		#region Unity Callbacks		
		/// <summary>
		/// Call this method repeatedly to update the time (typically, in a component's Update method).
		/// </summary>
		/// <param name="deltaTime">The delta time.</param>
		public void Update(float deltaTime)
		{
			if (IsPaused) return;
			if (IsDone) return;

			time -= deltaTime;
            
            // OBSOLETE (Added by Nrealus)
            /*
            if(OnClockUpdated != null)
                OnClockUpdated.Invoke();
            */

            CheckIfClockExpired();
            
			timeInSeconds.Value = Mathf.CeilToInt(time);
		}

        #endregion

        #region Public Methods		
        /// <summary>
        /// Adds time to this clock. This will extend the time the clock expires by
        /// the given amount.
        /// </summary>
        /// <param name="timeIncrement">The time increment.</param>
        public void AddTime(float timeIncrement)
		{
			time += timeIncrement;

			CheckIfClockExpired();
		}

		/// <summary>
		/// Resets the clock to the specified stat time.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <exception cref="ArgumentException"><c>startTime</c> is negative.</exception>
		public void Reset(float startTime)
		{
            // Removed by Nrealus
            /*
            if (startTime < 0)
			{
				throw new ArgumentException("Must be non-negative", "startTime");
			}
            */
            
            time = startTime;

            timeInSeconds.Value = Mathf.CeilToInt(startTime);

            CheckIfClockExpired(); // Added by Nrealus

            //Added by Nrealus
            if (time > 0)
                IsDone = false;
            else if (time == 0)
                IsDone = true;

            // Nrealus modifications : 
            // The idea is that if you reset to a negative value, the expiration callback will be fired,
            // the timer will then be set to 0. (and IsDone will be set to true)
            // If you reset to the value 0, nothing will happen except that IsDone will be set to true;
            // If you reset to a >0 value, everything will play out as expected.

        }

        /// <summary>
        /// Unpauses this clock.
        /// </summary>
        public void Unpause()
		{
			IsPaused = false;
		}

		/// <summary>
		/// Pauses this clock.
		/// </summary>
		public void Pause()
		{
			IsPaused = true;
		}

		#endregion

		#region Private Methods

		private void CheckIfClockExpired()
		{
			if (time < 0) // Nrealus : was a <= before, but we want the expiration callback to fire only once
			{
				time = 0;
				IsDone = true;

				if (OnClockExpired != null)
				{
					OnClockExpired();
				}
				else
				{
					//Debug.Log("OnClockExpired");
				}
			}
		}

		public void NotifySecondsChanged()
		{
			if (OnSecondsChanged != null)
			{
				OnSecondsChanged();
			}
		}
		#endregion
	}
}