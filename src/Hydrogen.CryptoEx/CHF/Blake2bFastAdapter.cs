﻿using System;
using System.Collections.Generic;
using System.Text;
using Blake2Fast;

namespace Hydrogen.CryptoEx;
public class Blake2bFastAdapter : HashFunctionBase {
	IBlake2Incremental _hasher;

	public Blake2bFastAdapter(int digestSize) {
		DigestSize = digestSize;
	}

	public override int DigestSize { get; }

	bool _calledInit = false;
	public override void Initialize() {
		_calledInit = true;
		base.Initialize();
		_hasher = Blake2Fast.Blake2b.CreateIncrementalHasher(DigestSize);
	}

	public override void Transform(ReadOnlySpan<byte> data) {
		base.Transform(data);
		_hasher.Update(data);

	}

	protected override void Finalize(Span<byte> digest) {
		_hasher.Finish(digest);
	}

	public override object Clone() {
		throw new NotSupportedException();
	}
}

public class Blake2sFastAdapter : HashFunctionBase {
	IBlake2Incremental _hasher;

	public Blake2sFastAdapter(int digestSize) {
		DigestSize = digestSize;
	}

	public override int DigestSize { get; }

	public override void Initialize() {
		base.Initialize();
		_hasher = Blake2Fast.Blake2s.CreateIncrementalHasher(DigestSize);
	}

	public override void Transform(ReadOnlySpan<byte> data) {
		base.Transform(data);
		_hasher.Update(data);

	}

	protected override void Finalize(Span<byte> digest) {
		_hasher.Finish(digest);
	}

	public override object Clone() {
		throw new NotSupportedException();
	}
}