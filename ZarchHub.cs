using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System;
using System.Text;
using System.Threading;

public class ZarchHub
{
    /// <summary>
    /// 成功继续执行 错误会抛异常
    /// </summary>
    /// <param name="url">网络地址</param>
    /// <param name="callback">参数是下载保存的path文件地址</param>
    public static void LoadResource(string url, Action<string> callback)
    {
        instance.Load_Resource(url, callback);
    }

    #region Download

    public void Load_Resource(string url, Action<string> callback)
    {
        init();

        string filepath;

        if (isReg(url))
            filepath = RegGet(url);
        else
        {
            filepath = GenFileName(FileNameLength > 4 ? FileNameLength : 4);
            Reg(url, filepath);
        }

        if (callback != null)
            data_callback[url] = callback;

        if (!data_thread.ContainsKey(url))
        {
            Thread thread = new Thread(new ParameterizedThreadStart(DownloadWorker));

            data_thread[url] = thread;

            thread.Start(url);

        }

    }

    void DownloadWorker(object url)
    {
        string _url = url.ToString();

        try
        {
            string _filepath = RegGet(_url);
            
            Stream write;

            long start_position = 0;

            if (File.Exists(_filepath))
            {
                write = File.OpenWrite(_filepath);

                start_position = write.Length;

                write.Seek(start_position, SeekOrigin.Current);
            }
            else
            {
                write = new FileStream(_filepath, FileMode.Create);
            }

            try
            {

                HttpWebRequest httpWebRequest = WebRequest.Create(_url) as HttpWebRequest;

                if (start_position != 0)
                {
                    httpWebRequest.AddRange(start_position);
                }

                Stream read = httpWebRequest.GetResponse().GetResponseStream();

                byte[] buffer = new byte[512];



                int read_size = read.Read(buffer, 0, buffer.Length);

                while (read_size > 0)
                {
                    write.Write(buffer, 0, read_size);
                    read_size = read.Read(buffer, 0, buffer.Length);
                }


                write.Close();

                read.Close();


                if (data_callback.ContainsKey(_url))
                    ThreadInvoker.InvokeInMainThread(delegate { data_callback[_url](_filepath); data_callback.Remove(_url); });


            }
            catch (Exception e)
            {
                write.Close();

                throw new Exception(e.ToString());
            }

            unReg(_url);

            data_thread.Remove(_url);
            
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());

            data_thread.Remove(_url);

            Thread.CurrentThread.Abort();
        }

        Thread.CurrentThread.Abort();

    }

    #endregion

    #region Manager

    // 网络中断的断点续传 删除缓存 等 回调存为Zarch语言
    void Reg(string url, string fileName)
    {
        data_map[url] = fileName;
    }

    void unReg(string url)
    {
        data_map.Remove(url);
    }

    bool isReg(string url)
    {
        return data_map.ContainsKey(url);
    }

    string RegGet(string url)
    {
        return data_map[url];
    }

    Dictionary<string, string> data_map = new Dictionary<string, string>();

    Dictionary<string, Action<string>> data_callback = new Dictionary<string, Action<string>>();

    Dictionary<string, Thread> data_thread = new Dictionary<string, Thread>();

    #endregion

    #region GenFileName

    string GenFileName(int Length)
    {
        builder.Clear();

        for (int i = 0; i < Length; i++)
        {
            builder.Append(Elements[r.Next(Elements.Length)]);
        }

        string result = Path.Combine(Location, Folder, builder.ToString()) ;

        if (File.Exists(result))
            return GenFileName(Length);

        return result;

    }

    char[] Elements = { 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                                   'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                                   '0','1','2','3','4','5','6','7','8','9'};

    StringBuilder builder = new StringBuilder();

    System.Random r = new System.Random();

    #endregion

    #region Internal
    static ZarchHub instance = new ZarchHub();

    private ZarchHub() { }

    void init()
    {
        if (!Directory.Exists(Path.Combine(Location, Folder)))
            Directory.CreateDirectory(Path.Combine(Location, Folder));
    }
    #endregion

    #region Config

    public static int FileNameLength = 9;

    public static string Folder = "ZarchHub";

    public static string Location = Application.persistentDataPath;

    #endregion

}
