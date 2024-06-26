﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT NON-AI software license, see the accompanying file
// LICENSE or visit https://sphere10.com/legal/NON-AI-MIT.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System.ComponentModel;

namespace Hydrogen;

public enum CHF : ushort {
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] [EditorBrowsable(EditorBrowsableState.Never)]
	ConcatBytes = 0,
	SHA2_256 = 1,
	SHA2_384,
	SHA2_512,
	SHA1_160,
	Blake2b_512,
	Blake2b_384,
	Blake2b_256,
	Blake2b_224,
	Blake2b_160,
	Blake2b_128,
	Blake2s_256,
	Blake2s_224,
	Blake2s_128,
	Blake2s_160,
	Blake2b_512_Fast,
	Blake2b_384_Fast,
	Blake2b_256_Fast,
	Blake2b_224_Fast,
	Blake2b_160_Fast,
	Blake2b_128_Fast,
	Blake2s_256_Fast,
	Blake2s_224_Fast,
	Blake2s_160_Fast,
	Blake2s_128_Fast,
	RIPEMD,
	RIPEMD_128,
	RIPEMD_160,
	RIPEMD_256,
	RIPEMD_320,
	Gost,
	Gost3411_2012_256,
	Gost3411_2012_512,
	Grindahl256,
	Grindahl512,
	Has160,
	Haval_3_128,
	Haval_3_160,
	Haval_3_192,
	Haval_3_224,
	Haval_3_256,
	Haval_4_128,
	Haval_4_160,
	Haval_4_192,
	Haval_4_224,
	Haval_4_256,
	Haval_5_128,
	Haval_5_160,
	Haval_5_192,
	Haval_5_224,
	Haval_5_256,
	Keccak_224,
	Keccak_256,
	Keccak_288,
	Keccak_384,
	Keccak_512,
	MD2,
	MD4,
	MD5,
	Panama,
	RadioGatun32,
	SHA0,
	SHA2_224,
	SHA2_512_224,
	SHA2_512_256,
	SHA3_224,
	SHA3_256,
	SHA3_384,
	SHA3_512,
	Snefru_8_128,
	Snefru_8_256,
	Tiger_3_128,
	Tiger_3_160,
	Tiger_3_192,
	Tiger_4_128,
	Tiger_4_160,
	Tiger_4_192,
	Tiger_5_128,
	Tiger_5_160,
	Tiger_5_192,
	Tiger2_3_128,
	Tiger2_3_160,
	Tiger2_3_192,
	Tiger2_4_128,
	Tiger2_4_160,
	Tiger2_4_192,
	Tiger2_5_128,
	Tiger2_5_160,
	Tiger2_5_192,
	WhirlPool

}
