﻿// Copyright (c) Sphere 10 Software. All rights reserved. (https://sphere10.com)
// Author: Herman Schoenfeld
//
// Distributed under the MIT software license, see the accompanying file
// LICENSE or visit http://www.opensource.org/licenses/mit-license.php.
//
// This notice must not be removed when duplicating this file or its contents, in whole or in part.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Hydrogen.Web.AspNetCore;

[XmlRoot("urlset", Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9")]
public class SitemapXml {
	private Dictionary<string, Node> _nodes = new();

	public bool HasNode(string url) => _nodes.ContainsKey(url);

	public void Add(string url, DateTime? lastModified = null, Frequency? frequency = null, double? priority = null)
		=> _nodes.Add(
			url,
			new Node {
				Url = url,
				LastModified = lastModified,
				Frequency = frequency,
				Priority = priority
			}
		);

	[XmlElement("url", typeof(Node))]
	public Node[] Nodes {
		get => _nodes.Values.ToArray();
		set => _nodes = value != null ? value.ToDictionary(x => x.Url) : new();
	}

	public class Node {

		[XmlElement("loc")] public string Url { get; set; }

		[XmlElement("lastmod", DataType = "date")]
		public DateTime? LastModified { get; set; }

		[XmlElement("changefreq")] public Frequency? Frequency { get; set; }

		[XmlElement("priority")] public double? Priority { get; set; }

		public bool LastModifiedSpecified => LastModified != null;

		public bool FrequencySpecified => Frequency != null;

		public bool PrioritySpecified => Priority != null;

	}

	public enum Frequency {

		[XmlEnum("never")] Never,

		[XmlEnum("yearly")] Yearly,

		[XmlEnum("monthly")] Monthly,

		[XmlEnum("weekly")] Weekly,

		[XmlEnum("daily")] Daily,

		[XmlEnum("hourly")] Hourly,

		[XmlEnum("always")] Always
	}

}
