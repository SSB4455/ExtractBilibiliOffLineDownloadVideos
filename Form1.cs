using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
	public partial class Form1 : Form
	{
		public string topPath;
		string newName = "";



		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//ListFiles(new DirectoryInfo(textBox1.Text));
			Std(textBox1.Text);
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{

		}

		public void ListFiles(FileSystemInfo info)
		{
			if (!info.Exists) return;
			DirectoryInfo dir = info as DirectoryInfo;
			//不是目录 
			if (dir == null)
			{
				return;
			}
			FileSystemInfo[] files = dir.GetFileSystemInfos();
			List<FileInfo> mp4File = new List<FileInfo>();
			for (int i = 0; i < files.Length; i++)
			{
				string newName2 = newName;
				FileInfo file = files[i] as FileInfo;
				//是文件 
				if (file != null)
				{
					if (file.FullName.EndsWith("entry.json"))
					{
						Console.WriteLine(file.FullName + "\t " + file.Length);
						Hashtable json = MiniJSON.jsonDecode(File.ReadAllText(file.FullName)) as Hashtable;
						if (json["title"] != null)
						{
							newName2 = json["title"].ToString();
							if (json["page_data"] != null)
							{
								Hashtable page_data = json["page_data"] as Hashtable;
								if (page_data["part"] != null)
								{
									newName2 += page_data["part"].ToString();
									Console.WriteLine("part");
								}
							}
							Console.WriteLine("title");
							if (newName2 != newName)
							{
								newName = newName2;
							}
						}
						Console.WriteLine("ssstt = " + json["title"] + "\t" + newName);
						//newName = MiniJSON.jsonDecode(file.FullName).ToString().Split(new string[1]{"title"},StringSplitOptions.None)[1],;
					}
					if (System.IO.Path.GetExtension(file.FullName) == ".mp4")
					{
						mp4File.Add(file);
					}
				}
				//对于子目录，进行递归调用 
				else
				{
					ListFiles(files[i]);
				}
			}

			if (mp4File.Count > 0)
			{
				newName = newName.Replace("<", "_").Replace(">", "_").Replace("/", "_").Replace("\\", "_").Replace(":", "_").Replace("*", "_").Replace("?", "_");
				for (int i = 0; i < mp4File.Count; i++)
				{
					if (!File.Exists(textBox1.Text + newName + (mp4File.Count > 1 ? i.ToString() : "") + ".mp4"))
					{
						Console.WriteLine("<文件名>" + newName + (mp4File.Count > 1 ? i.ToString() : "") + ".mp4");
						try
						{
							File.Move(mp4File[i].FullName, textBox1.Text + newName + (mp4File.Count > 1 ? i.ToString() : "") + ".mp4");
						}
						catch
						{
							Random rd = new Random();
							int r = rd.Next();
							File.Move(mp4File[i].FullName, textBox1.Text + r + (mp4File.Count > 1 ? i.ToString() : "") + ".mp4");
						}
					}
				}
			}
		}

		void Std(string dirPath)
		{
			DirectoryInfo dir = new DirectoryInfo(dirPath);
			if (!dir.Exists)
			{
				return;
			}
			FileInfo[] files = dir.GetFiles("entry.json");
			FileInfo entryFileinfo = null;
			if (files != null && files.Length > 0)
			{
				entryFileinfo = files[0];
				RnameVideoFromEntryFile(entryFileinfo);
			}
			else
			{
				DirectoryInfo[] dirs = dir.GetDirectories();
				for (int i = 0; i < dirs.Length; i++)
				{
					Std(dirs[i].FullName);
				}
			}
		}

		string[] RnameVideoFromEntryFile(FileInfo entryFileInfo)
		{
			Console.WriteLine("entryFile = " + entryFileInfo.FullName);
			List<string> videoFileAddresss = new List<string>();
			DirectoryInfo entryFileDir = entryFileInfo.Directory;
			FileInfo[] files = entryFileDir.GetFiles("*", SearchOption.AllDirectories);
			for (int i = 0; i < files.Length; i++)
			{
				if (files[i].Name.EndsWith(".blv"))
				{
					videoFileAddresss.Add(files[i].FullName);
				}
				else if (files[i].Name.EndsWith(".flv"))
				{
					videoFileAddresss.Add(files[i].FullName);
				}
				else if (files[i].Name.EndsWith(".mp4"))
				{
					videoFileAddresss.Add(files[i].FullName);
				}
			}

			for (int i = 0; i < videoFileAddresss.Count; i++)
			{
				Console.WriteLine("videoFileAddresss = " + videoFileAddresss[i]);
			}

			return videoFileAddresss.ToArray();
		}
	}
}
