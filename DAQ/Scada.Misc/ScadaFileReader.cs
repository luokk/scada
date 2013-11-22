using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Scada.Files
{
	public enum ReadLineResult
	{
		Null,
		Error,
		OK
	}

	public enum SectionType
	{
		None,
		Spaces,
		Title,
		Comment,
		KeyWithStringValue,
		KeyWithArrayValue,
		KeyWithDictValue,
	}

	public enum ScadaValueType
	{
		Null,
		String,
		Array,
		Dict
	}

	public interface IValue
	{
		ScadaValueType Type { get; }
	}

	public class StringValue : IValue
	{
		private string value = null;

		public StringValue(string value)
		{
			this.value = value;
		}

		public ScadaValueType Type
		{
			get { return ScadaValueType.String; }
		}

		public override string ToString()
		{
			return this.value;
		}
	}

	public class ArrayValue : IValue
	{
		private List<string> list = new List<string>();
		public void Add(string item)
		{
			list.Add(item);
		}
		public int Count
		{
			get { return list.Count; }
		}
		public string Item(int index)
		{
			return list[index];
		}

		public ScadaValueType Type
		{
			get { return ScadaValueType.Array; }
		}
	}

	public class DictValue : IValue
	{
		public ScadaValueType Type
		{
			get { return ScadaValueType.Dict; }
		}
	}



	class ScadaReader : IDisposable
	{
		private StreamReader streamReader = null;

		public ScadaReader(string fileName)
		{
			if (File.Exists(fileName))
			{
				this.streamReader = new StreamReader(fileName);
			}
		}

		public ScadaReader(Stream stream)
		{
			this.streamReader = new StreamReader(stream);
		}

		public ReadLineResult ReadLine(out SectionType secType, out string line, out string key, out IValue value)
		{
			secType = SectionType.None;
			line = string.Empty;
			key = string.Empty;
			value = null;
			if (streamReader == null)
			{
				return ReadLineResult.Error;
			}
			line = streamReader.ReadLine();
			if (line == null)
			{
				return ReadLineResult.Null;
			}
			if (line == string.Empty)
			{
				return ReadLineResult.OK;
			}
			string trimmedLine = line.Trim();
			if (trimmedLine == string.Empty)
			{
				secType = SectionType.Spaces;
				return ReadLineResult.OK;
			}
			// line = trimmedLine;
			return this.ParseLine(trimmedLine, ref secType, out key, out value);
		}

		private ReadLineResult ParseLine(string line, ref SectionType secType, out string key, out IValue value)
		{
			key = string.Empty;
			value = null;
			// Comment
			int commentStart = line.IndexOf("#");
			if (commentStart == 0)
			{
				secType = SectionType.Comment;
				return ReadLineResult.OK;
			}
			// Title
			int leftBracketPos = line.IndexOf("[");
			if (leftBracketPos == 0)
			{
				int rightBracketPos = line.IndexOf("]");
				if (rightBracketPos > 0)
				{
					string content = line.Substring(leftBracketPos + 1, rightBracketPos - leftBracketPos - 1);
					content = content.Trim();
					if (IsValidKey(content))
					{
						secType = SectionType.Title;
						key = content;
						return ReadLineResult.OK;
					}
				}
				return ReadLineResult.Error;
			}
			// Assignment
			int assignPos = line.IndexOf("=");
			if (assignPos > 0)
			{
				string assignBefore = line.Substring(0, assignPos).Trim();
				if (IsValidKey(assignBefore))
				{
					key = assignBefore;
					string assignAfter = line.Substring(assignPos + 1).Trim();
					if (assignAfter.StartsWith("[") && assignAfter.EndsWith("]"))
					{
						this.ParseArray(assignAfter, out value);
						return ReadLineResult.Error;
					}
					else if (assignAfter.StartsWith("{") && assignAfter.EndsWith("}"))
					{
						
						return ReadLineResult.Error;
					}
					else
					{
						if (assignAfter.StartsWith("<<"))
						{
							string delimiter = assignAfter.Substring(2).Trim();
							StringBuilder sb = new StringBuilder();
							string oneLine = streamReader.ReadLine();
							while (oneLine != null)
							{
								if (oneLine != delimiter)
								{
									sb.Append(oneLine).Append('\n');
									oneLine = streamReader.ReadLine();
								}
								else
								{
									break;
								}
							}
							secType = SectionType.KeyWithStringValue;
							value = new StringValue(sb.ToString());
							return ReadLineResult.OK;
						}
					}
				}
				return ReadLineResult.Error;
			}
			return ReadLineResult.Error;
		}


		private static bool IsValidKey(string key)
		{
			foreach (char ch in key)
			{
				if (!(char.IsLetterOrDigit(ch) || ch == '_' || ch == '-'))
				{
					return false;
				}
			}
			return true;
		}

		private bool ParseArray(string content, out IValue value)
		{
			ArrayValue array = new ArrayValue();
			content = content.Substring(1, content.Length - 2);
			
			StringBuilder sb = new StringBuilder();
			bool inQuote = false;
			for (int i = 0; i < content.Length; ++i)
			{
				char ch = content[i];
				if (inQuote)
				{
					if (ch == '"')
					{
						if (i > 0 && content[i - 1] == '\\')
						{
							sb.Append('"');
						}
						else
						{
							array.Add(sb.ToString());
							sb.Length = 0;
							inQuote = false;
						}
					}
					else
					{
						sb.Append(ch);
					}
				}
				else
				{
					if (ch == '"')
					{
						inQuote = true;
					}
					// Skip comma, and spaces.
				}
		
			}
			value = array;
			return true;
		}


		public void Dispose()
		{
			if (streamReader != null)
			{
				streamReader.Close();
				streamReader = null;
			}
		}
	}
}
