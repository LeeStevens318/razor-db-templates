using System.Collections.Concurrent;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace RazorDbTemplates
{
	/// <summary>
	/// 
	/// </summary>
	public class InMemoryFileProvider : IFileProvider
	{
		/// <summary>
		/// 
		/// </summary>
		private readonly ConcurrentDictionary<string, IFileInfo> _files = new();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="content"></param>
		public void AddFile(string path, string content)
		{
			_files.TryAdd(path, new InMemoryFileInfo(path, content));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subpath"></param>
		/// <returns></returns>
		public IFileInfo GetFileInfo(string subpath)
		{
			_files.TryGetValue(subpath, out var file);
			
			return file ?? new NotFoundFileInfo(subpath);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="subpath"></param>
		/// <returns></returns>
		public IDirectoryContents GetDirectoryContents(string subpath) => 
			new NotFoundDirectoryContents();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filter"></param>
		/// <returns></returns>
		public IChangeToken Watch(string filter) => 
			NullChangeToken.Singleton;
	}

	/// <summary>
	/// 
	/// </summary>
	public class InMemoryFileInfo : IFileInfo
	{
		/// <summary>
		/// 
		/// </summary>
		public bool Exists { get; }

		/// <summary>
		/// 
		/// </summary>
		public long Length { get; }

		/// <summary>
		/// 
		/// </summary>
		public string PhysicalPath { get; }

		/// <summary>
		/// 
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// 
		/// </summary>
		public DateTimeOffset LastModified => DateTimeOffset.UtcNow;

		/// <summary>
		/// 
		/// </summary>
		public bool IsDirectory { get; }

		/// <summary>
		/// 
		/// </summary>
		private readonly byte[] _data;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		/// <param name="content"></param>
		public InMemoryFileInfo(string path, string content)
		{
			_data = System.Text.Encoding.UTF8.GetBytes(content);

			PhysicalPath = path;

			Name = Path.GetFileName(path);

			IsDirectory = false;

			Exists = true;

			Length = _data.Length;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Stream CreateReadStream() => 
			new MemoryStream(_data);
	}
}
