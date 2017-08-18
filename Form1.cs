using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace WindowsFormsApplication1
{
	public partial class Form1 : Form
	{
		public string topPath;
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ListFiles(new DirectoryInfo(textBox1.Text));
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{

		}
		string newName = "";
		public void ListFiles(FileSystemInfo info)
		{
			if (!info.Exists) return;
			DirectoryInfo dir = info as DirectoryInfo;
			//不是目录 
			if (dir == null) return;
			FileSystemInfo[] files = dir.GetFileSystemInfos();
			List<FileInfo> mp4File=new List<FileInfo>();
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
							if(json["page_data"]!=null)
							{
								Hashtable page_data = json["page_data"] as Hashtable;
								if (page_data["part"] != null)
								{
									newName2 += page_data["part"].ToString();
									System.Console.WriteLine("part");
								}
							}
							System.Console.WriteLine("title");
							if(newName2!=newName)
							{
								newName = newName2;
							}
						}
						System.Console.WriteLine("ssstt = " + json["title"] + "\t" + newName);
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
					newName.Replace("<","_");
					newName.Replace(">", "_");
					newName.Replace("/", "_");
					newName.Replace("\\", "_");
					newName.Replace(":", "_");
					newName.Replace("*", "_");
					newName.Replace("?", "_");
				for (int i = 0; i < mp4File.Count; i++)
				{
					if (!File.Exists(textBox1.Text + newName + (mp4File.Count > 1 ? i.ToString() : "") + ".mp4"))
					{
						System.Console.WriteLine("<文件名>" + newName + (mp4File.Count > 1 ? i.ToString() : "") + ".mp4");
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
	} 
}
