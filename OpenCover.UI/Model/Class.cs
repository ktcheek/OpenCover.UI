﻿//
// This source code is released under the GPL License; Please read license.md file for more details.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
	/// <summary>
	/// An entity that contains methods
	/// </summary>
	public class Class : SkippedEntity
	{
		public Class()
		{
			Methods = new Method[0];
			Summary = new Summary();
		}

		/// <summary>
		/// A Summary of results for a class
		/// </summary>
		public Summary Summary { get; set; }

		/// <summary>
		/// Control serialization of the Summary  object
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeSummary() { return !ShouldSerializeSkippedDueTo(); }

		/// <summary>
		/// The full name of the class
		/// </summary>
		public string FullName { get; set; }
		
		[XmlIgnore]
		internal File[] Files { get; set; }

		/// <summary>
		/// A list of methods that make up the class
		/// </summary>
		public Method[] Methods { get; set; }

		public override void MarkAsSkipped(SkippedMethod reason)
		{
			SkippedDueTo = reason;
		}

		public string Name
		{
			get
			{
				int lastIndex = FullName.LastIndexOf(".");
				string name = lastIndex > 0 ? FullName.Substring(lastIndex + 1) : FullName;

				return name;
			}
		}

		public string Namespace
		{
			get
			{
				int lastIndex = FullName.LastIndexOf(".");
				string @namespace = lastIndex > 0 ? FullName.Substring(0, lastIndex) : FullName;

				return @namespace;
			}
		}

		public IEnumerable<IGrouping<uint, SequencePoint[]>> GetSequencePoints()
		{
			if (Methods != null)
			{
				return Methods
							.Where(methods => methods.FileRef != null)
							.GroupBy(methods => methods.FileRef.UniqueId, methods => methods.SequencePoints);
			}

			return null;
		}
	}
}
