﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Hydrogen;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class TransientAttribute : Attribute {
}