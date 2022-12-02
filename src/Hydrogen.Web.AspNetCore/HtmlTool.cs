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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Hydrogen;
using Hydrogen;
using Hydrogen.Web.AspNetCore.AnimateCss;
using Microsoft.AspNetCore.Mvc.Rendering;

// ReSharper disable CheckNamespace
namespace Tools.Web {

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

		public static SelectList ToSelectList<TEnum>(object selectedItem = default)  where TEnum : Enum
			=> ToSelectList(typeof(TEnum), selectedItem);

		public static SelectList ToSelectList(Type enumType, object selectedItem) {
			List<SelectListItem> items = new List<SelectListItem>();
			foreach (Enum item in Enum.GetValues(enumType)) {
				FieldInfo fi = enumType.GetField(item.ToString());
				//var attribute =  fi.GetCustomAttributes(typeof(DescriptionAttribute), true).FirstOrDefault();
				var title = Tools.Enums.GetDescription(item); //  attribute == null ? item.ToString() : ((DescriptionAttribute)attribute).Description;
				var listItem = new SelectListItem {
					Value = item.ToString(),
					Text = title,
					Selected = selectedItem switch { null => false, _ => selectedItem.Equals(item) }
				};
				items.Add(listItem);
			}

			return new SelectList(items, "Value", "Text");
		}



		public static string Beautify(object obj) {

			var val = string.Empty;
			if (obj != null) {
				TypeSwitch.For(obj,
					TypeSwitch.Case<string>(s => val = s),
					TypeSwitch.Case<DateTime?>(dt => val = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt)),
					TypeSwitch.Case<DateTime>(dt => val = string.Format("{0:yyyy-MM-dd HH:mm:ss}", dt)),
					TypeSwitch.Case<byte[]>(b => val =  b.ToHexString(true).ToUpperInvariant()),
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