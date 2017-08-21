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
			DirectoryInfo entryFileDir = entryFileInfo.Directory;
			FileInfo[] files = entryFileDir.GetFiles("*", SearchOption.AllDirectories);
			List<FileInfo> videoFiles = new List<FileInfo>();
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].Name.EndsWith(".blv") || files[i].Name.EndsWith(".flv") || files[i].Name.EndsWith(".mp4"))
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
				videoEntry.fileInfo = videoFiles[i];
				videoEntry.title = title;
				videoEntry.part = part;
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
					string newPath = outPutDir + Path.DirectorySeparatorChar + videoEntrys[0].title + videoEntrys[0].fileInfo.Extension;
					File.Move(videoEntrys[0].fileInfo.FullName, newPath);
				}
				else if (videoEntrys.Count > 1)
				{
					string newDirectoryPath = outPutDir + Path.DirectorySeparatorChar + videoEntrys[0].title;
					Directory.CreateDirectory(newDirectoryPath);
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
						for (int j = 0; j < videoEntrys.Count; j++)
						{
							string newFilePart1 = newDirectoryPath + Path.DirectorySeparatorChar + videoEntrys[j].title;
							File.Move(videoEntrys[j].fileInfo.FullName, newFilePart1 + videoEntrys[j].fileInfo.Name);
						}
						//生成自动批处理合成配置文件 最后并执行
						GenertMergeVideoPlayerList(newDirectoryPath);
					}
					else if (partList.Count > 1)
					{
						for (int j = 0; j < partList.Count; j++)
						{
							newDirectoryPath = outPutDir + Path.DirectorySeparatorChar + videoEntrys[0].title;
							if (videoEntryByPartList[partList[j]].Count == 1)
							{
								string newFilePart1 = newDirectoryPath + Path.DirectorySeparatorChar + videoEntrys[j].part;
								File.Move(videoEntrys[j].fileInfo.FullName, newFilePart1 + videoEntrys[j].fileInfo.Extension);
							}
							else if (videoEntryByPartList[partList[j]].Count > 1)
							{
								newDirectoryPath = outPutDir + Path.DirectorySeparatorChar + videoEntrys[0].title
									+ Path.DirectorySeparatorChar + videoEntrys[j].part;
								for (int k = 0; k < videoEntryByPartList[partList[j]].Count; k++)
								{
									string newFilePart1 = newDirectoryPath + Path.DirectorySeparatorChar + videoEntryByPartList[partList[j]][k].title;
									File.Move(videoEntryByPartList[partList[j]][k].fileInfo.FullName, newFilePart1 + videoEntryByPartList[partList[j]][k].fileInfo.Name);
								}
								//生成自动批处理合成配置文件 最后并执行
								GenertMergeVideoPlayerList(newDirectoryPath);
							}
						}
					}
				}
			}
		}

		string GetWindowsCanUseName(string orgName)
		{
			return orgName.Replace("<", "_").Replace(">", "_").Replace("/", "_").Replace("\\", "_").Replace(":", "_").Replace("*", "_").Replace("?", "_");
		}

		bool GenertMergeVideoPlayerList(string dirPath)
		{
			DirectoryInfo videosDir = new DirectoryInfo(dirPath);
			if (!videosDir.Exists)
			{
				return false;
			}

			string playerListStr = "";
			FileInfo[] files = videosDir.GetFiles("*");
			for (int i = 0; i < files.Length; i++)
			{
				playerListStr += "file '" + files[i].FullName + ((i < files.Length - 1) ? "'\n" : "");
			}
			string playerlistFilePath = dirPath + Path.DirectorySeparatorChar + "playlist";
			File.WriteAllText(playerlistFilePath, playerListStr);


			System.Diagnostics.Process p = new System.Diagnostics.Process();
			p.StartInfo.FileName = "cmd.exe";
			p.StartInfo.UseShellExecute = false;			//是否使用操作系统shell启动
			p.StartInfo.RedirectStandardInput = true;		//接受来自调用程序的输入信息
			p.StartInfo.RedirectStandardOutput = false;		//由调用程序获取输出信息
			p.StartInfo.RedirectStandardError = false;		//重定向标准错误输出
			p.StartInfo.CreateNoWindow = true;				//不显示程序窗口
			p.Start();										//启动程序

			//向cmd窗口发送输入信息
			p.StandardInput.WriteLine(videosDir.Root.ToString()[0] + ":");
			p.StandardInput.WriteLine("cd " + videosDir);
			p.StandardInput.WriteLine("ffmpeg -f concat -safe 0 -i playlist -c copy outVideo.mp4 &exit");		//防止文件名无效
			p.StandardInput.AutoFlush = true;

			p.WaitForExit();//等待程序执行完退出进程
			p.Close();
			Console.WriteLine("marge finish");

			File.Move(dirPath + Path.DirectorySeparatorChar + "outVideo.mp4", dirPath + ".mp4");
			videosDir.Delete(true);

			return true;
		}
	}











	internal class VideoEntry
	{
		internal int avid;
		internal FileInfo fileInfo;
		internal string title, part;



		internal VideoEntry(int avid)
		{
			this.avid = avid;
		}
	}
}
