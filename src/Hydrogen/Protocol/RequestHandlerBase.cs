﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;

namespace Hydrogen.Communications;

public abstract class RequestHandlerBase : IRequestHandler {
	public abstract object Execute(ProtocolOrchestrator orchestrator, object request);

	public abstract Type RequestType { get; }
	public abstract Type ResponseType { get; }
}
