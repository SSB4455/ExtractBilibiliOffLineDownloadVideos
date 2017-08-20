using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace extractBilibiliOffLineDownloadVideos
{
	public partial class Form1 : Form
	{



		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ScanTheDirectory(textBox1.Text);
		}

		public void ScanTheDirectory(string dirPath)
		{
			DirectoryInfo dir = new DirectoryInfo(dirPath);
			if (!dir.Exists)
			{
				return;
			}

			Dictionary<int, List<VideoEntry>> videoEntryByAvidList = new Dictionary<int, List<VideoEntry>>();
			FileInfo[] files = dir.GetFiles("entry.json", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				List<VideoEntry> videoEntrys = ScanVideoFromEntryFile(files[i]);
				if (videoEntrys != null && videoEntrys.Count > 0)
				{
					if (!videoEntryByAvidList.ContainsKey(videoEntrys[0].avid))
					{
						videoEntryByAvidList.Add(videoEntrys[0].avid, new List<VideoEntry>());
					}
					videoEntryByAvidList[videoEntrys[0].avid].AddRange(videoEntrys);
				}
			}

			MoveVideoFiles(videoEntryByAvidList, dirPath);
		}

		List<VideoEntry> ScanVideoFromEntryFile(FileInfo entryFileInfo)
		{
			Console.WriteLine("entryFile = " + entryFileInfo.FullName);
			List<FileInfo> videoFiles = new List<FileInfo>();
			DirectoryInfo entryFileDir = entryFileInfo.Directory;
			FileInfo[] files = entryFileDir.GetFiles("*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].Name.EndsWith(".blv"))
				{
					videoFiles.Add(files[i]);
				}
				else if (files[i].Name.EndsWith(".flv"))
				{
					videoFiles.Add(files[i]);
				}
				else if (files[i].Name.EndsWith(".mp4"))
				{
					videoFiles.Add(files[i]);
				}
			}

			Hashtable json = MiniJSON.jsonDecode(File.ReadAllText(entryFileInfo.FullName)) as Hashtable;
			if (string.IsNullOrEmpty(json["title"].ToString()))
			{
				return null;
			}
			int avid = (int)((double)json["avid"]);
			string title = GetWindowsCanUseName(json["title"].ToString());
			string part = GetWindowsCanUseName((json["page_data"] as Hashtable)["part"].ToString());
			List<VideoEntry> videoEntrys = new List<VideoEntry>();
			for (int i = 0; i < videoFiles.Count; i++)
			{
				//Console.WriteLine("videoFileAddresss = " + videoFiles[i].FullName);
				VideoEntry videoEntry = new VideoEntry(avid);
				videoEntry.videoPath = videoFiles[i].FullName;
				videoEntry.title = title;
				videoEntry.part = part;
				videoEntry.extension = videoFiles[i].Extension;
				videoEntrys.Add(videoEntry);
			}
			return videoEntrys;
		}

		private void MoveVideoFiles(Dictionary<int, List<VideoEntry>> videoEntryByAvidList, string outPutDir)
		{
			List<int> videoIdList = new List<int>(videoEntryByAvidList.Keys);
			for (int i = 0; i < videoIdList.Count; i++)
			{
				List<VideoEntry> videoEntrys = new List<VideoEntry>(videoEntryByAvidList[videoIdList[i]]);
				if (videoEntrys.Count == 1)
				{
					string newPath = outPutDir + Path.DirectorySeparatorChar + videoEntrys[0].title + videoEntrys[0].extension;
					File.Move(videoEntrys[0].videoPath, newPath);
				}
				else if (videoEntrys.Count > 1)
				{
					Dictionary<string, List<VideoEntry>> videoEntryByPartList = new Dictionary<string, List<VideoEntry>>();
					for (int j = 0; j < videoEntrys.Count; j++)
					{
						if (!videoEntryByPartList.ContainsKey(videoEntrys[j].part))
						{
							videoEntryByPartList.Add(videoEntrys[j].part, new List<VideoEntry>());
						}
						videoEntryByPartList[videoEntrys[j].part].Add(videoEntrys[j]);
					}
					List<string> partList = new List<string>(videoEntryByPartList.Keys);
					if (partList.Count == 1)
					{
						string newDirectoryPath = outPutDir + Path.DirectorySeparatorChar + videoEntrys[0].title;
						Directory.CreateDirectory(newDirectoryPath);
						for (int j = 0; j < videoEntrys.Count; j++)
						{
							string newPath = newDirectoryPath + Path.DirectorySeparatorChar + videoEntrys[j].title + "_" + j + videoEntrys[j].extension;
							File.Move(videoEntrys[j].videoPath, newPath);
						}
						//生成自动批处理合成配置文件 最后并执行
					}
					// 多part
					// 多part 每个部分要带title+part 每个part看是否是多文件 多文件按每个part生成文件夹 
				}
			}
		}

		string GetWindowsCanUseName(string orgName)
		{
			return orgName.Replace("<", "_").Replace(">", "_").Replace("/", "_").Replace("\\", "_").Replace(":", "_").Replace("*", "_").Replace("?", "_");
		}
	}











	internal class VideoEntry
	{
		internal int avid;
		internal string videoPath, title, part, extension;



		internal VideoEntry(int avid)
		{
			this.avid = avid;
		}
	}
}
