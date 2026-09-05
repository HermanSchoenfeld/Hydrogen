# Sphere10.Framework.Consensus — Design Document

**Author:** Herman Schoenfeld  
**Status:** Living document — expand as the library grows

---

## 1. Purpose and Philosophy

This library provides the generic, reusable building blocks that any proof-of-work or state-transition blockchain needs. The goal is to be **protocol-agnostic**: the abstractions here should work equally well for Bitcoin-style UTXO chains, account-model chains (Ethereum-style), or entirely custom ledger designs.

Three concerns are kept strictly separate:

| Concern | What it covers |
|---|---|
| **State transition** | How a block mutates ledger state; how that mutation can be undone |
| **Chain management** | Ordering of finalized blocks; branching and re-org of unfinalized blocks |
| **Proof-of-work** | Compact target encoding; difficulty adjustment algorithms |

These concerns compose but do not couple. A chain may use any difficulty algorithm, and a difficulty algorithm knows nothing about ledger state.

---

## 2. Core Abstractions — State Transitions

### 2.1 Operation\<TState\>

The atomic unit of state change. Every mutation of ledger state is modelled as an `Operation`.

```
Operation<TState>
  Apply(TState state)   -- mutate state forward
  Undo(TState state)    -- reverse the mutation exactly
```

Operations must be **fully invertible**: `Undo(Apply(s)) == s`. This is the invariant the entire undo/re-org machinery depends on.

Examples of operations in a UTXO chain:
- Spend a coin output (Apply marks it spent; Undo unmarks it)
- Create a new coin output (Apply inserts it; Undo removes it)
- Update an account balance (Apply sets new balance; Undo restores old balance)

### 2.2 Block\<TState\>

A block is an ordered sequence of operations that are applied atomically.

```
Block<TState>
  Operations : Operation<TState>[]
  Apply(TState state)   -- applies each operation in order
  Undo(TState state)    -- reverses each operation in reverse order
```

`Block.Apply` and `Block.Undo` are convenience wrappers that iterate `Operations` in forward and reverse order respectively. The state must be left identical after `Apply` then `Undo`.

### 2.3 IBlockchainState

The ledger state object that `Operation` and `Block` act upon. The only contract imposed by this framework is an **update scope** — a disposable region inside which mutations may be batched or rolled back as a unit.

```
IBlockchainState
  EnterUpdateScope() : IDisposable
    -- Opens a logical transaction on the state.
    -- Disposing without an explicit commit rolls back.
```

Concrete implementations decide what "rollback" means — it may be a database transaction, an in-memory snapshot, or a journal file. The framework does not mandate the mechanism.

---

## 3. Core Abstractions — Chain Management

### 3.1 IBlockchain\<TBlock, TState, TBlockID, TWeight, TOperationID\>

The core interface for a linear blockchain. Neither finalized nor unfinalized — those semantics are provided by decorators.

```
IBlockchain<TBlock, TState, TBlockID, TWeight, TOperationID>
  State : TState
  WeightAggregator : IWeightAggregator<TWeight>
  Height : long
  Blocks : IReadOnlyList<LinkedBlock>
  Head : LinkedBlock
  AggregatedWeight : TWeight
  ApplyBlock(block : TBlock)
  UndoBlock() : LinkedBlock
  event BlockApplied
  event BlockUndone
```

### 3.2 BlockchainBase\<TBlock, TState, TBlockID, TWeight, TOperationID\>

Abstract base implementing `IBlockchain` with event wiring. Subclasses implement `ApplyBlock`/`UndoBlock`.

### 3.3 BlockchainDecorator\<TBlock, TState, TBlockID, TWeight, TOperationID, TConcrete\>

Decorator pattern following `ExtendedListDecorator`. Routes all calls to `InternalBlockchain` (typed as `TConcrete`). Subclasses override specific methods to add behavior.

### 3.4 Blockchain\<TBlock, TState, TBlockID, TWeight, TOperationID\>

Concrete linear chain extending `BlockchainBase`. Maintains a list of `LinkedBlock` entries, each with an `AggregatedWeight` computed via `WeightAggregator`.

```
Blockchain
  ApplyBlock(block)
    -- computes AggregatedWeight = WeightAggregator.Aggregate(Head.AggregatedWeight, block.Weight)
    -- creates LinkedBlock(block, parentID, hasParent, aggregatedWeight)
    -- applies block inside State.EnterUpdateScope()
```

### 3.5 UnfinalizedBlockGraph\<TBlock, TState, TBlockID, TWeight, TOperationID\>

Models the **unfinalized, potentially branching** frontier. Multiple competing tip blocks may exist simultaneously; each represents a candidate for the next finalized head.

```
UnfinalizedBlockGraph
  Nodes : Dictionary<TBlockID, LinkedBlock>
  PotentialHeads : Dictionary<TBlockID, LinkedBlock>
  SetFinalizedBlock(block)           -- sets root, clears all potential heads
  Add(block, previousBlockID)        -- adds child, replaces parent as head
  RemoveHead(blockID)
  GetPathToRoot(blockID)
  GetPathFromAncestor(ancestorID, descendantID)
  FindCommonAncestor(xID, yID)
  IsDescendantOf(descendantID, ancestorID)
  PruneToPath(retainedIDs)
```

When a new block arrives it is added as a child of its parent by ID. The new block's `AggregatedWeight` is computed by aggregating the parent's `AggregatedWeight` with the block's own `Weight`. This forms a tree of candidate futures rooted at the current finalized head.

### 3.6 FinalizedBlockchain\<TBlock, TState, TBlockID, TWeight, TOperationID\>

A `BlockchainDecorator` wrapping a `Blockchain` that tracks finalization. Maintains an `UnfinalizedBlockGraph` whose root is always synced to the finalized head.

```
FinalizedBlockchain : BlockchainDecorator<Blockchain>
  UnfinalizedSector : UnfinalizedBlockGraph
  AddUnfinalizedRoot(block)
  AddUnfinalizedBlock(block, previousBlockID)
  FinalizePath(winningHeadID)
  CreateReorgPlan(fromHeadID, toHeadID)
  ExecuteReorg(plan)
```

**Invariants:**

- The unfinalized sector's root is always the finalized chain's `Head`.
- Finalizing a tip applies each block in order onto the finalized chain, then re-roots the unfinalized sector at the new head.
- Re-orgs undo finalized blocks back to the common ancestor, then re-apply the winning branch, then re-root the unfinalized sector.

This decorator design keeps the linear chain logic separate from finalization/branching logic, following the same pattern as `ExtendedList` / `ExtendedListDecorator`.

---

## 4. Proof-of-Work Primitives

### 4.1 ICompactTargetAlgorithm — Target Encoding

A mining target is a 256-bit threshold: a block hash must be numerically below it to be valid. Storing and comparing 256-bit numbers everywhere is impractical, so targets are compressed to a 32-bit **compact target**.

```
ICompactTargetAlgorithm
  MinCompactTarget : uint           -- easiest (lowest) difficulty
  MaxCompactTarget : uint           -- hardest (highest) difficulty

  FromTarget(BigInteger)  : uint    -- compress a full 256-bit target
  ToTarget(uint)          : BigInteger
  FromDigest(byte[32])    : uint    -- compress a raw hash digest
  ToDigest(uint, Span)             -- expand compact back to 32-byte digest
  AggregateWork(uint, uint) : uint  -- accumulate chain work
```

**Critical property:** the compact representation must be **orderable** — a higher `uint` value must always mean harder difficulty. This allows simple integer comparison for chain-work comparison.

#### MolinaTargetAlgorithm

The provided implementation uses the encoding invented by Albert Molina for PascalCoin. It differs from Bitcoin's `nBits`:

- The 8 most-significant bits store the count of leading zero bits in the full target.
- The remaining 24 bits store the bitwise-inverted significant mantissa, preserving the orderable property.
- Result: larger `uint` value always equals harder difficulty — safe to compare directly.

Range: `134217728` (easiest) to `3892314111` (hardest).

### 4.2 IDAAlgorithm — Difficulty Adjustment

After each block the network must agree on the target for the next block. The difficulty adjustment algorithm (DAA) computes this from recent block timestamps and the previous compact target.

```
IDAAlgorithm
  RealTime : bool
  CalculateNextBlockTarget(
      previousBlockTimestamps : IEnumerable<DateTime>,
      previousCompactTarget   : uint,
      blockNumber             : uint
  ) : uint
```

`RealTime = true` means the algorithm uses the wall-clock time of the calling node (suited for mining). `RealTime = false` means it uses only committed block timestamps (suited for deterministic validation).

#### ASERT Formula

Both implementations use the Absolutely Scheduled Exponentially Rising Targets (ASERT) formula:

```
nextTarget = previousTarget × exp( (Δt − T) / τ )
```

| Symbol | Meaning |
|---|---|
| Δt | Observed time delta (seconds) |
| T  | Desired block time (seconds) — from `ASERTConfiguration.BlockTime` |
| τ  | Relaxation (half-life) time (seconds) — from `ASERTConfiguration.RelaxationTime` |

The exponential is computed with a fixed-point approximation to avoid floating-point non-determinism across platforms.

**Behaviour:**
- Block found faster than `T` → target rises (harder)
- Block found slower than `T` → target falls (easier)
- Large `τ` → slow, smooth adjustments; small `τ` → fast but potentially oscillating

#### ASERT_RTT (Real-Time Target)

`Δt = now − headBlockTime`

Uses the current wall-clock time. Appropriate during active mining where the miner adjusts difficulty in real time. `RealTime = true`.

#### ASERT2 (Block-to-Block)

`Δt = headBlockTime − (head−1)BlockTime`

Uses only committed timestamps. Fully deterministic and suitable for validating historical blocks. Requires at least two timestamps; returns `MinCompactTarget` if fewer are available. `RealTime = false`.

### 4.3 ASERTConfiguration

```
ASERTConfiguration
  BlockTime      : TimeSpan   -- target interval between blocks (e.g. 60 s)
  RelaxationTime : TimeSpan   -- smoothing half-life (e.g. 1 h)
```

These are the only two parameters both ASERT variants need. They are deliberately separate from any chain-level configuration so the same algorithm objects can be shared across test and production contexts.

---

## 5. Supporting Utilities

### PeriodicStatistics

Tracks per-period statistics (count, mean, variance) for a rolling window of historical periods. Used internally to track observed hash rates and block timing, but available for general-purpose instrumentation.

```
PeriodicStatistics(period, historyLength)
  Start()
  RegisterEvent(magnitude)
  RegisterEvent(magnitude, occurrences)
  PeriodsAvailable : int
  StartedOn        : DateTime
```

### CryptoTool (planned)

Commented-out helpers for:
- `DeriveSecureChecksum(secret)` — a compact, non-revealing 32-bit checksum of a secret via double-SHA256. Useful for fast secret lookups without exposure.
- `DeriveChildDigest(digest, index)` — hierarchical key derivation: `H(H(i ‖ seed))`, safe against length-extension attacks.

These will be uncommented and completed when the broader cryptographic identity layer is introduced.

---

## 6. Layering and Extension Points

```
┌─────────────────────────────────────────────────────────┐
│                  Protocol / Application                  │
│   (defines TState, TBlock, consensus rules, p2p, etc.)  │
├─────────────────────────────────────────────────────────┤
│              FinalizedBlockchain                       │
│   FinalizedSector (Blockchain)                          │
│   UnfinalizedSector (UnfinalizedBlockGraph)               │
├──────────────────────┬──────────────────────────────────┤
│  State Transition    │  Proof-of-Work                   │
│  Block<TState>       │  ICompactTargetAlgorithm         │
│  Operation<TState>   │  IDAAlgorithm                    │
│  IBlockchainState    │  ASERTConfiguration              │
└──────────────────────┴──────────────────────────────────┘
```

To build a new chain on top of this library:

1. **Define `TState`** implementing `IBlockchainState` with whatever ledger data your protocol requires.
2. **Define `TBlock`** implementing `Block<TState>`, decomposing protocol transactions into `Operation<TState>` instances.
3. **Choose or implement `ICompactTargetAlgorithm`** (use `MolinaTargetAlgorithm` unless you need Bitcoin `nBits` compatibility).
4. **Choose `IDAAlgorithm`** — `ASERT2` for deterministic validation, `ASERT_RTT` for mining; tune `ASERTConfiguration` for your network's target block time.
5. **Wire `FinalizedBlockchain`** to your p2p block-receive and finalization logic.

---

## 7. Design Decisions and Trade-offs

| Decision | Rationale |
|---|---|
| Generic `TState` / `TBlock` | The framework imposes no opinion on ledger model (UTXO, account, custom). The protocol layer owns the state. |
| `Operation.Undo` on every op | Enables O(1) re-orgs without replaying the entire chain from genesis. |
| Two-sector chain model | Clear invariant boundary between settled and candidate state simplifies both validation and persistence. |
| Fixed-point ASERT exp | Floating-point is non-deterministic across CPU/OS. Fixed-point gives identical results on all nodes. |
| Molina compact target (orderable) | Direct `uint` comparison for chain-work is simpler and less error-prone than Bitcoin's non-orderable `nBits`. |
| `IBlockchainState.EnterUpdateScope` | Decouples the framework from any specific persistence or rollback mechanism (in-memory, SQL, file journal). |

---

## 8. Planned Work

- [x] Flesh out `IBlockchainState`, `Operation<TState>`, and `Block<TState>` as concrete interfaces/base classes in code.
- [x] Implement `Blockchain<TBlock, TState>` (concrete linear chain) with full Apply/Undo stack.
- [x] Implement `UnfinalizedBlockGraph<TBlock, TState>` with tree-based potential-head tracking.
- [x] Implement `FinalizedBlockchain` with re-org support.
- [x] Re-enable and complete `CryptoTool` helpers.
- [x] Add Merkle-root computation helper (bridging to `Sphere10.Framework` Merkle tree support).
- [x] Add reference chain implementation (simple account-model) to serve as integration test and usage example.

