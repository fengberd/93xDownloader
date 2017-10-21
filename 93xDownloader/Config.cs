using System.IO;
using System.Text;
using System.Collections.Generic;

public class Config : Dictionary<string,string>
{
    public string this[string key,string setDefault]
    {
        get
        {
            if(!ContainsKey(key))
            {
                this[key] = setDefault;
                Save();
            }
            return this[key];
        }
    }

    public string Path = "";

    public Config(string path)
    {
        Path = path;
        if(!File.Exists(path))
        {
            File.Create(path).Close();
        }
        using(StreamReader reader = new StreamReader(File.OpenRead(path),Encoding.UTF8))
        {
            while(!reader.EndOfStream)
            {
                string[] split = reader.ReadLine().Split(new char[] { '=' },2);
                if(split.Length == 2)
                {
                    if(split[1].Length >= 2 && split[1].StartsWith("\"") && split[1].EndsWith("\"") && !split[1].EndsWith("\\\""))
                    {
                        split[1] = split[1].Substring(1,split[1].Length - 2);
                    }
                    this[split[0]] = split[1].Replace("\\r","\r").Replace("\\n","\n");
                }
            }
        }
    }

    public void Save()
    {
        using(StreamWriter sw = new StreamWriter(File.Open(Path,FileMode.OpenOrCreate,FileAccess.Write,FileShare.Read),Encoding.UTF8))
        {
            sw.BaseStream.SetLength(0);
            foreach(KeyValuePair<string,string> pair in this)
            {
                sw.WriteLine(pair.Key + "=" + pair.Value.Replace("\r","\\r").Replace("\n","\\n"));
            }
        }
    }

    public int GetInt(string key)
    {
        return int.Parse(this[key]);
    }

    public int GetInt(string key,int setDefault)
    {
        return int.Parse(this[key,setDefault.ToString()]);
    }

    public bool GetBool(string key)
    {
        return bool.Parse(this[key]);
    }

    public bool GetBool(string key,bool setDefault)
    {
        return bool.Parse(this[key,setDefault.ToString()]);
    }
}
