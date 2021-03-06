﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Aiml.Tags {
	/// <summary>Represents an <code>oob</code> or rich media tag. These are not fully parsed during template processing.</summary>
	/// <remarks>
	///	    <code>oob</code> and rich media tags provide out-of-band instructions to the client in the response.
	///     The <code>oob</code> element is defined by the AIML 2.0 specification.
	///     Rich media elements are defined by the AIML 2.1 draft specification.
	/// </remarks>
	public sealed class Oob : RecursiveTemplateTag {
		public string Name { get; }

		public Oob(string name, TemplateElementCollection? children) : base(children) {
			this.Name = name;
		}

		public override string Evaluate(RequestProcess process) {
			if (this.Children != null)
				return $"<{this.Name}>{this.Children?.Evaluate(process) ?? ""}</{this.Name}>";
			else
				return $"<{this.Name}/>";
		}

		public static Oob FromXml(XmlNode node, AimlLoader loader, params string[] childTags) {
			if (node.HasChildNodes) {
				var children = new List<TemplateNode>();
				foreach (XmlNode node2 in node.ChildNodes) {
					if (node2.NodeType == XmlNodeType.Whitespace) {
						children.Add(new TemplateText(" "));
					} else if (node2.NodeType == XmlNodeType.Text || node2.NodeType == XmlNodeType.SignificantWhitespace) {
						children.Add(new TemplateText(node2.InnerText));
					} else if (node2.NodeType == XmlNodeType.Element) {
						if (Array.IndexOf(childTags, node2.Name) >= 0) {
							children.Add(Oob.FromXml(node2, loader, node2.Name.Equals("card", StringComparison.InvariantCultureIgnoreCase) ? new[] { "title", "subtitle", "image", "button" } :
								node2.Name.Equals("button", StringComparison.InvariantCultureIgnoreCase) ? new[] { "text", "postback", "url" } : new string[0]));
						} else
							children.Add(loader.ParseElement(node2));
					}
				}
				return new Oob(node.Name, new TemplateElementCollection(children.ToArray()));
			} else
				return new Oob(node.Name, null);
		}
	}
}
