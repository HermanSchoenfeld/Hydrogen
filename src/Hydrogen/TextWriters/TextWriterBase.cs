//-----------------------------------------------------------------------
// <copyright file="TextWriterBase.cs" company="Sphere 10 Software">
//
// Copyright (c) Sphere 10 Software. All rights reserved. (http://www.sphere10.com)
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// <author>Herman Schoenfeld</author>
// <date>2018</date>
// </copyright>
//-----------------------------------------------------------------------

using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Hydrogen;


public abstract class TextWriterBase : TextWriter {


	public sealed override void Write(char[] buffer, int index, int count) {
		InternalWrite(new string(buffer, index, count));
	}

	public sealed override void Write(string value) {
		InternalWrite(value);
	}

	public sealed override Task WriteAsync(string value)
		=> InternalWriteAsync(value);

	public override Task WriteAsync(char[] buffer, int index, int count) 
		=> InternalWriteAsync(new string(buffer, index, count));

	protected abstract void InternalWrite(string value);

	protected abstract Task InternalWriteAsync(string value);

	public override Encoding Encoding => Encoding.Default;
}