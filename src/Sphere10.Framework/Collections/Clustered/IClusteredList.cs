﻿using System;
using System.Collections.Generic;
using System.IO;

namespace Sphere10.Framework {

	public interface IClusteredList<TItem> : IExtendedList<TItem> {
		IClusteredStorage Storage { get; }
	}


}
