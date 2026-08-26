// Copyright (c) Herman Schoenfeld 2018 - Present. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using NUnit.Framework;
using Sphere10.Framework.Consensus;
using Sphere10.Framework.Consensus.Tests.ReferenceChain;
using System.Linq;

namespace Sphere10.Framework.Consensus.Tests;

[TestFixture]
[Parallelizable(ParallelScope.Children)]
public class UnfinalizedBlockGraphTests {

	private static UnfinalizedBlockGraph<AccountBlock, AccountState, int, long, int> CreateGraph() {
		var graph = new UnfinalizedBlockGraph<AccountBlock, AccountState, int, long, int>(new LongWeightAggregator());
		graph.SetFinalizedBlock(new AccountBlock(new AccountOperation("genesis", 0)));
		return graph;
	}

	[Test]
	public void SetFinalizedBlock_SetsRoot() {
		var graph = new UnfinalizedBlockGraph<AccountBlock, AccountState, int, long, int>(new LongWeightAggregator());
		var block = new AccountBlock(new AccountOperation("alice", 100));

		graph.SetFinalizedBlock(block);

		Assert.That(graph.Nodes, Has.Count.EqualTo(1));
		Assert.That(graph.PotentialHeads, Has.Count.EqualTo(0));
		Assert.That(graph.Nodes[block.ID].IsRoot, Is.True);
	}

	[Test]
	public void SetFinalizedBlock_ClearsExistingHeads() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);
		graph.Add(new AccountBlock(new AccountOperation("carol", 25)), rootID);

		Assert.That(graph.PotentialHeads, Has.Count.EqualTo(2));

		graph.SetFinalizedBlock(new AccountBlock(new AccountOperation("newroot", 0)));

		Assert.That(graph.Nodes, Has.Count.EqualTo(1));
		Assert.That(graph.PotentialHeads, Has.Count.EqualTo(0));
	}

	[Test]
	public void Add_ReplacesParentAsPotentialHead() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		var child = graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);

		Assert.That(graph.PotentialHeads, Has.Count.EqualTo(1));
		Assert.That(graph.PotentialHeads[child.Block.ID], Is.SameAs(child));
		Assert.That(graph.PotentialHeads.ContainsKey(rootID), Is.False);
	}

	[Test]
	public void Add_MultipleChildrenOfSameParent() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		var childA = graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);
		var childB = graph.Add(new AccountBlock(new AccountOperation("carol", 25)), rootID);

		Assert.That(graph.PotentialHeads, Has.Count.EqualTo(2));
		Assert.That(graph.Nodes, Has.Count.EqualTo(3));
	}

	[Test]
	public void Add_ThrowsWhenParentNotFound() {
		var graph = CreateGraph();
		var block = new AccountBlock(new AccountOperation("bob", 50));

		Assert.That(() => graph.Add(block, 99999), Throws.InvalidOperationException);
	}

	[Test]
	public void RemoveHead_RemovesFromPotentialHeads() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		var child = graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);

		Assert.That(graph.PotentialHeads, Has.Count.EqualTo(1));

		var removed = graph.RemoveHead(child.Block.ID);

		Assert.That(removed, Is.True);
		Assert.That(graph.PotentialHeads, Has.Count.EqualTo(0));
	}

	[Test]
	public void GetPathToRoot_ReturnsOrderedPath() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		var root = graph.Nodes[rootID];
		var child = graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);
		var grandchild = graph.Add(new AccountBlock(new AccountOperation("carol", 25)), child.Block.ID);

		var path = graph.GetPathToRoot(grandchild.Block.ID);

		Assert.That(path, Has.Count.EqualTo(3));
		Assert.That(path[0], Is.SameAs(root));
		Assert.That(path[1], Is.SameAs(child));
		Assert.That(path[2], Is.SameAs(grandchild));
	}

	[Test]
	public void FindCommonAncestor_ReturnsCorrectNode() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		var root = graph.Nodes[rootID];
		var branchA = graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);
		var branchB = graph.Add(new AccountBlock(new AccountOperation("carol", 25)), rootID);

		var ancestor = graph.FindCommonAncestor(branchA.Block.ID, branchB.Block.ID);

		Assert.That(ancestor, Is.SameAs(root));
	}

	[Test]
	public void FindCommonAncestor_NoCommonAncestor_ReturnsNull() {
		var graph1 = CreateGraph();
		var graph2 = new UnfinalizedBlockGraph<AccountBlock, AccountState, int, long, int>(new LongWeightAggregator());
		graph2.SetFinalizedBlock(new AccountBlock(new AccountOperation("other", 0)));

		var ancestor = graph1.FindCommonAncestor(graph1.Nodes.Keys.First(), graph2.Nodes.Keys.First());

		Assert.That(ancestor, Is.Null);
	}

	[Test]
	public void PruneToPath_RemovesNonRetainedNodes() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		var root = graph.Nodes[rootID];
		var child1 = graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);
		var child2 = graph.Add(new AccountBlock(new AccountOperation("carol", 25)), rootID);

		graph.PruneToPath(new[] { root.Block.ID, child1.Block.ID });

		Assert.That(graph.Nodes, Has.Count.EqualTo(2));
		Assert.That(graph.Nodes.ContainsKey(root.Block.ID), Is.True);
		Assert.That(graph.Nodes.ContainsKey(child1.Block.ID), Is.True);
		Assert.That(graph.Nodes.ContainsKey(child2.Block.ID), Is.False);
	}

	[Test]
	public void IsDescendantOf_WorksCorrectly() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		var root = graph.Nodes[rootID];
		var child = graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);
		var grandchild = graph.Add(new AccountBlock(new AccountOperation("carol", 25)), child.Block.ID);

		Assert.That(graph.IsDescendantOf(grandchild.Block.ID, root.Block.ID), Is.True);
		Assert.That(graph.IsDescendantOf(grandchild.Block.ID, child.Block.ID), Is.True);
		Assert.That(graph.IsDescendantOf(child.Block.ID, grandchild.Block.ID), Is.False);
		Assert.That(graph.IsDescendantOf(root.Block.ID, child.Block.ID), Is.False);
	}

	[Test]
	public void AggregatedWeight_CalculatesCorrectly() {
		var graph = CreateGraph();
		var rootID = graph.Nodes.Keys.First();
		var root = graph.Nodes[rootID];
		var child = graph.Add(new AccountBlock(new AccountOperation("bob", 50)), rootID);
		var grandchild = graph.Add(new AccountBlock(new AccountOperation("carol", 25)), child.Block.ID);

		Assert.That(root.AggregatedWeight, Is.EqualTo(1L));
		Assert.That(child.AggregatedWeight, Is.EqualTo(2L));
		Assert.That(grandchild.AggregatedWeight, Is.EqualTo(3L));
	}
}
