// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.Numerics;

namespace Sphere10.Framework.Consensus;

public static class ICompactTargetAlgorithmExtensions {
	public static byte[] ToDigest(this ICompactTargetAlgorithm alg, uint compactTaget) {
		var bytes = new byte[32];
		alg.ToDigest(compactTaget, bytes);
		return bytes;

	}

	public static byte[] ToDigest(this ICompactTargetAlgorithm alg, BigInteger target) {
		var bytes = new byte[32];
		alg.ToDigest(target, bytes);
		return bytes;
	}
}
