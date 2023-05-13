﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Hydrogen;
using Hydrogen.Communications;
using Hydrogen.DApp.Core.Consensus;
using Hydrogen.DApp.Core.Maths;
using Hydrogen.DApp.Core.Mining;

namespace Hydrogen.DApp.Node.RPC {

	[Serializable]
	public class MiningSolutionJsonSurogate {
		[JsonConverter(typeof(HexadecimalValueConverterReader))]
		public uint WorkID { get; set; }
		public string MinerTag { get; set; }
		[JsonConverter(typeof(HexadecimalValueConverterReader))]
		public uint		Nonce { get; set; }
		[JsonConverter(typeof(HexadecimalValueConverterReader))]
		public uint		Time { get; set; }
	}
}
