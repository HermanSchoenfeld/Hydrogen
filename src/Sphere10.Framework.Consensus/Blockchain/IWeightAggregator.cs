// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

namespace Sphere10.Framework.Consensus;

/// <summary>
/// Aggregates a block's weight with its parent's aggregated weight.
/// Used to compute cumulative chain work / difficulty.
/// </summary>
public interface IWeightAggregator<TWeight> {

	/// <summary>
	/// Aggregates a block's weight with the aggregated weight of its parent.
	/// </summary>
	/// <param name="parentAggregatedWeight">The aggregated weight of the parent block.</param>
	/// <param name="blockWeight">The weight of the current block.</param>
	/// <returns>The aggregated weight for the current block.</returns>
	TWeight Aggregate(TWeight parentAggregatedWeight, TWeight blockWeight);
}
