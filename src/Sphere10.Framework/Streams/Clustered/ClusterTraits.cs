﻿using System;

namespace Sphere10.Framework {
	[Flags]
	internal enum ClusterTraits : byte {
		First = 1 << 0,
		Listing = 1 << 1,
		Data = 1 << 2,
	}
}
