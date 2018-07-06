﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace JsonSettings
{
	public enum Scope
	{
		Application,
		User
	}

	public abstract class JsonSettingsBase
	{
		/// <summary>
		/// Indicates whether the settings file is scoped to the user or application
		/// </summary>
		[JsonIgnore]
		public abstract Scope Scope { get; }
		/// <summary>
		/// Override this to set Company Name associated with your product's settings file. This will become part of the save location for the settings file
		/// </summary>
		[JsonIgnore]
		public abstract string CompanyName { get; }
		/// <summary>
		/// Override this to set Product Name associated with your product's settings file. This will become part of the save location for the settings file
		/// </summary>
		[JsonIgnore]
		public abstract string ProductName { get; }
		/// <summary>
		/// Set this to the filename (with no path information) for the settings file
		/// </summary>
		[JsonIgnore]
		public abstract string Filename { get; }

		public static T Load<T>() where T : JsonSettingsBase, new()
		{
			T result = new T();

			string fileName = result.GetFullPath();
			if (File.Exists(fileName))
			{
				using (StreamReader reader = File.OpenText(fileName))
				{
					JsonSerializerSettings settings = new JsonSerializerSettings()
					{						
						ContractResolver = new DataProtectionResolver()
					};

					string json = reader.ReadToEnd();
					if (!json.Equals(string.Empty))
					{
						result = JsonConvert.DeserializeObject<T>(json, settings);
					}
				}
			}

			return result;
		}

		public string GetFullPath()
		{
			Dictionary<Scope, Environment.SpecialFolder> paths = new Dictionary<Scope, Environment.SpecialFolder>()
			{
				{ Scope.Application, Environment.SpecialFolder.LocalApplicationData },
				{ Scope.User, Environment.SpecialFolder.Personal }
			};

			string result = Path.Combine(Environment.GetFolderPath(paths[Scope]), CompanyName, ProductName, Filename);

			string folder = Path.GetDirectoryName(result);
			if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

			return result;
		}

		public void Save()
		{			
			using (StreamWriter writer = File.CreateText(GetFullPath()))
			{
				JsonSerializerSettings settings = new JsonSerializerSettings()
				{
					Formatting = Formatting.Indented,
					ContractResolver = new DataProtectionResolver()
				};
				string json = JsonConvert.SerializeObject(this, settings);
				writer.Write(json);
			}
		}
	}
}