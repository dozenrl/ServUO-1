#region Header
//               _,-'/-'/
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2023  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #                                       #
#endregion

#region References
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Server;
#endregion

namespace VitaNex
{
	public sealed class ClilocTable : IEnumerable<ClilocData>, IDisposable
	{
		private readonly Dictionary<int, ClilocData> _Table = new Dictionary<int, ClilocData>();

		public int Count => _Table.Count;

		public ClilocLNG Language { get; private set; }
		public FileInfo InputFile { get; private set; }

		public bool Loaded { get; private set; }

		public ClilocInfo this[int index] => Lookup(index);

		public void Dispose()
		{
			Unload();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _Table.Values.GetEnumerator();
		}

		public IEnumerator<ClilocData> GetEnumerator()
		{
			return _Table.Values.GetEnumerator();
		}

		public void Clear()
		{
			foreach (var d in _Table.Values)
			{
				d.Clear();
			}

			_Table.Clear();
		}

		public void Unload()
		{
			if (!Loaded)
			{
				return;
			}

			Language = ClilocLNG.NULL;

			InputFile = null;

			Clear();

			Loaded = false;
		}

		public void Load(FileInfo file)
		{
			if (Loaded)
			{
				if (Insensitive.Equals(file?.FullName, InputFile?.FullName))
				{
					return;
				}

				Clear();
			}

			VitaNexCore.TryCatch(f =>
			{
				if (!Enum.TryParse(f.Extension.TrimStart('.'), true, out ClilocLNG lng))
				{
					throw new FileLoadException($"Could not detect language for: {f.FullName}");
				}

				Language = lng;
				InputFile = f;

				InputFile.Deserialize(reader =>
				{
					var size = reader.Seek(0, SeekOrigin.End);

					if (size <= 0)
					{
						throw new InvalidDataException("Cliloc file is empty or corrupted");
					}

					reader.Seek(0, SeekOrigin.Begin);

					reader.ReadInt();
					reader.ReadShort();

					var errors = 0;

					while (reader.Seek(0, SeekOrigin.Current) < size)
					{
						try
						{
							if (reader.Seek(0, SeekOrigin.Current) + 7 > size)
							{
								break;
							}

							var index = reader.ReadInt();

							reader.ReadByte();

							int length = reader.ReadShort();

							if (length < 0)
							{
								errors++;
								if (errors <= 8)
								{
									Utility.PushColor(ConsoleColor.Red);
									Console.WriteLine($"Error loading cliloc file {f.FullName}: Invalid cliloc entry length: {length}");
									Utility.PopColor();
								}

								break;
							}

							var offset = reader.Seek(0, SeekOrigin.Current);

							if (offset + length > size)
							{
								errors++;
								if (errors <= 8)
								{
									Utility.PushColor(ConsoleColor.Red);
									Console.WriteLine($"Error loading cliloc file {f.FullName}: Cliloc entry extends beyond file bounds");
									Utility.PopColor();
								}

								break;
							}

							_Table[index] = new ClilocData(Language, index, offset, length);
							reader.Seek(length, SeekOrigin.Current);
						}
						catch (EndOfStreamException)
						{
							break;
						}
						catch (Exception ex)
						{
							errors++;
							if (errors <= 8)
							{
								Utility.PushColor(ConsoleColor.Red);
								Console.WriteLine($"Error loading cliloc file {f.FullName}: {ex.Message}");
								Utility.PopColor();
							}

							break;
						}
					}
				});

				Loaded = true;
			}, file, Clilocs.CSOptions.ToConsole);
		}

		public bool Contains(int index)
		{
			return _Table.ContainsKey(index);
		}

		public bool IsNullOrWhiteSpace(int index)
		{
			if (!_Table.TryGetValue(index, out var data) || data == null)
			{
				return true;
			}

			var info = data.Lookup(InputFile);

			if (info == null || String.IsNullOrWhiteSpace(info.Text))
			{
				return true;
			}

			return false;
		}

		public ClilocInfo Update(int index)
		{
			if (!_Table.TryGetValue(index, out var data) || data == null)
			{
				return null;
			}

			return data.Lookup(InputFile, true);
		}

		public ClilocInfo Lookup(int index)
		{
			if (!_Table.TryGetValue(index, out var data) || data == null)
			{
				return null;
			}

			return data.Lookup(InputFile);
		}

		public void LookupAll()
		{
			VitaNexCore.TryCatch(() =>
			{
				InputFile.Deserialize(reader =>
				{
					foreach (var d in _Table.Values)
					{
						VitaNexCore.TryCatch(d.Load, reader, Clilocs.CSOptions.ToConsole);
					}
				});
			}, Clilocs.CSOptions.ToConsole);
		}

		public override string ToString()
		{
			return Language == ClilocLNG.NULL ? "Not Loaded" : $"Cliloc.{Language}";
		}
	}
}
