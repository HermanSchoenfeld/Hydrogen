﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;

namespace Hydrogen;

public interface IMemoryPage<TItem> : IPage<TItem>, IDisposable {

	/// <summary>
	/// The maximum byte size of the page.
	/// </summary>
	int MaxSize { get; set; }

	/// <summary>
	/// Saves the page to storage.
	/// </summary>
	void Save();

	/// <summary>
	/// Loads the page from storage.
	/// </summary>
	void Load();

	/// <summary>
	/// Unloads the page from memory.
	/// </summary>
	void Unload();

}
