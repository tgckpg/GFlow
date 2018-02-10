using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

using Net.Astropenguin.Logging;

namespace libtaotu.Models.Interfaces
{
	using Procedure;

	interface ICrawler
	{
		Task<IStorageFile> DownloadSource( string url );

		Action<string, LogType> Log { get; set; }
		Action<Procedure, string, LogType> PLog { get; set; }

		Task<ProcConvoy> Crawl( ProcConvoy Convoy );
	}
}