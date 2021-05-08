//-----------------------------------------------------------------------
// <copyright file="WebTool.cs" company="Sphere 10 Software">
//
// Copyright (c) Sphere 10 Software. All rights reserved. (http://www.sphere10.com)
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// <author>Herman Schoenfeld</author>
// <date>2018</date>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Sphere10.Framework;
using Sphere10.Framework.Html;

// ReSharper disable CheckNamespace
namespace Tools {

	public static partial class Html {

		public static string Percent(decimal value) => $"{value:P}";
		
		public static Animation[] EntryAnimationClasses = {
            #region Entry classes
            // Attenion Seekers
            //Animation.bounce,
            //Animation.flash,
            //Animation.pulse,
            //Animation.rubberBand,
            //Animation.shake,
            //Animation.swing,
            //Animation.tada,
            //Animation.wobble,
            //Animation.jello,

            // Bouncing Entrances
            Animation.bounceIn,
			Animation.bounceInDown,
			Animation.bounceInLeft,
			Animation.bounceInRight,
			Animation.bounceInUp,

            // Fading Entrances
            Animation.fadeIn,
			Animation.fadeInDown,
			Animation.fadeInDownBig,
			Animation.fadeInLeft,
			Animation.fadeInLeftBig,
			Animation.fadeInRight,
			Animation.fadeInRightBig,
			Animation.fadeInUp,
			Animation.fadeInUpBig,

            // Flippers
            Animation.flip,
			Animation.flipInX,
			Animation.flipInY,

            // Lightspeed
            Animation.lightSpeedIn,

            // Rotating Entraces
            Animation.rotateIn,
			Animation.rotateInDownLeft,
			Animation.rotateInDownRight,
			Animation.rotateInUpLeft,
			Animation.rotateInUpRight,

            // Sliding Entrances
            Animation.slideInUp,
			Animation.slideInDown,
			Animation.slideInLeft,
			Animation.slideInRight,

            // Specials
            Animation.rollIn,

            // Zoom Entrances 
            Animation.zoomIn,
			Animation.zoomInDown,
			Animation.zoomInLeft,
			Animation.zoomInRight,
			Animation.zoomInUp,

            #endregion
        };

		public static Animation[] ExitAnimationClasses = {
            #region Exit classes
            // Bouncing Exits   
            Animation.bounceOut,
			Animation.bounceOutDown,
			Animation.bounceOutLeft,
			Animation.bounceOutRight,
			Animation.bounceOutUp,

            // Fading Exits
            Animation.fadeOut,
			Animation.fadeOutDown,
			Animation.fadeOutDownBig,
			Animation.fadeOutLeft,
			Animation.fadeOutLeftBig,
			Animation.fadeOutRight,
			Animation.fadeOutRightBig,
			Animation.fadeOutUp,
			Animation.fadeOutUpBig,

            // Flippers
            Animation.flipOutX,
			Animation.flipOutY,

            // Rotating Exits
            Animation.rotateOut,
			Animation.rotateOutDownLeft,
			Animation.rotateOutDownRight,
			Animation.rotateOutUpLeft,
			Animation.rotateOutUpRight,

            // Sliding Exits
            Animation.slideOutUp,
			Animation.slideOutDown,
			Animation.slideOutLeft,
			Animation.slideOutRight,

            // Specials
            Animation.hinge,
			Animation.rollOut,

            // Zoom Exists
            Animation.zoomOut,
			Animation.zoomOutDown,
			Animation.zoomOutLeft,
			Animation.zoomOutRight,
			Animation.zoomOutUp
            #endregion
        };


		public static bool IsDebug() {
#if DEBUG
			return true;
#else
      return false;
#endif
		}

		public static string Beautify(object obj) {

			var val = string.Empty;
			if (obj != null) {
				TypeSwitch.Do(obj,
					TypeSwitch.Case<string>(s => val = s),
					TypeSwitch.Case<DateTime?>(dt => val = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt)),
					TypeSwitch.Case<DateTime>(dt => val = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt)),
					//TypeSwitch.Case<byte[]>(b => val =  BitcoinProtocolHelper.BytesToString(b)),
					TypeSwitch.Case<decimal?>(d => val = string.Format("{0:0.#############################}", d)),
					TypeSwitch.Case<decimal>(d => val = string.Format("{0:0.#############################}", d)),
					TypeSwitch.Case<double?>(d => val = string.Format("{0:0.#############################}", d)),
					TypeSwitch.Case<double>(d => val = string.Format("{0:0.#############################}", d)),
					TypeSwitch.Case<float?>(d => val = string.Format("{0:0.#############################}", d)),
					TypeSwitch.Case<float>(d => val = string.Format("{0:0.#############################}", d)),
					TypeSwitch.Default(() => val = obj.ToString())
					);
			} else {
				val = string.Empty;
			}
			return val;
		}


		public static string RandomEntryAnimationClass(AnimationDelay delay = AnimationDelay.Seconds_1_0) {
			return AnimationClass(Tools.Array.RandomElement(EntryAnimationClasses), delay);
		}

		public static string RandomExitAnimationClass(AnimationDelay delay = AnimationDelay.Seconds_1_0) {
			return AnimationClass(Tools.Array.RandomElement(ExitAnimationClasses), delay);
		}

		public static string AnimationClass(Animation animation, AnimationDelay delay = AnimationDelay.Seconds_1_0) {
			return string.Format("animated {0} {1}", Tools.Enums.GetDescription(animation), Tools.Enums.GetDescription(delay));
		}

	}
}
