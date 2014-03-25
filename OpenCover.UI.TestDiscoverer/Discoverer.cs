﻿//
// This source code is released under the MIT License;
//
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using OpenCover.UI.Model.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenCover.UI.TestDiscoverer
{
	/// <summary>
	/// Discovers tests in the given dlls.
	/// </summary>
	internal class Discoverer
	{
		private IEnumerable<string> _dlls;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestDiscoverer"/> class.
		/// </summary>
		/// <param name="dlls">The DLLS.</param>
		public Discoverer(IEnumerable<string> dlls)
		{
			_dlls = dlls;
		}

		/// <summary>
		/// Loads the assembly and all its referenced assemblies.
		/// </summary>
		/// <param name="dll">The DLL.</param>
		/// <returns>Loaded assembly</returns>
		private static AssemblyDefinition LoadAssembly(string dll)
		{
			//Assembly assembly = Assembly.ReflectionOnlyLoadFrom(dll);
			Directory.SetCurrentDirectory(Path.GetDirectoryName(dll));

			//Creates an AssemblyDefinition from the "MyLibrary.dll" assembly
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(dll);

			return assembly;
		}

		/// <summary>
		/// Discovers all tests in the selected assemblies.
		/// </summary>
		/// <returns></returns>
		public List<TestClass> Discover()
		{
			List<TestClass> tests = new List<TestClass>();

			if (_dlls != null)
			{
				foreach (var dll in _dlls)
				{
					tests.AddRange(DiscoverTestsInDLL(dll));
				}
			}

			return tests;
		}

		/// <summary>
		/// Discovers the tests in DLL.
		/// </summary>
		/// <param name="dll">The DLL.</param>
		/// <returns>Tests in the DLL</returns>
		private List<TestClass> DiscoverTestsInDLL(string dll)
		{
			var classes = new List<TestClass>();

			if (File.Exists(dll))
			{
				AssemblyDefinition assembly = null;
				try
				{
					assembly = LoadAssembly(dll);
				}
				catch { }

				if (assembly != null)
				{
					foreach (var type in assembly.MainModule.Types)
					{
						bool isTestClass = false;

						try
						{
							var customAttributes = type.CustomAttributes;
							if (customAttributes != null)
							{
								isTestClass = customAttributes.Any(attribute => attribute.AttributeType.FullName == typeof(TestClassAttribute).FullName);
							}
						}
						catch { }

						if (isTestClass)
						{
							var TestClass = new TestClass
							{
								DLLPath = dll,
								Name = type.Name,
								Namespace = type.Namespace
							};

							TestClass.TestMethods = DiscoverTestsInClass(type, TestClass);
							classes.Add(TestClass);
						}
					}
				}
			}

			return classes;
		}

		/// <summary>
		/// Discovers the tests in class.
		/// </summary>
		/// <param name="type">Type of the class.</param>
		/// <returns>Tests in the class</returns>
		private TestMethod[] DiscoverTestsInClass(TypeDefinition type, TestClass @class)
		{
			var tests = new List<TestMethod>();
			foreach (var method in type.Methods)
			{
				bool isTestMethod = false;
				var trait = new List<string>();

				try
				{
					foreach (var attribute in method.CustomAttributes)
					{
						if (attribute.AttributeType.FullName == typeof(TestMethodAttribute).FullName)
						{
							isTestMethod = true;
						}

						if (attribute.AttributeType.FullName == typeof(TestCategoryAttribute).FullName)
						{
							if (attribute.ConstructorArguments != null && attribute.ConstructorArguments.Count > 0)
							{
								trait.Add(attribute.ConstructorArguments[0].Value.ToString());
							}
						}
					}
				}
				catch { }

				if (isTestMethod)
				{
					TestMethod testMethod = new TestMethod();
					testMethod.Name = method.Name;
					testMethod.Traits = trait.Count > 0 ? trait.ToArray() : new[] { "No Traits" };
					tests.Add(testMethod);
				}
			}

			return tests.ToArray();
		}
	}
}
