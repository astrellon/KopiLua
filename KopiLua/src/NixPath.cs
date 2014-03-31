using System.Collections;
using System.Collections.Generic;

namespace KopiLua
{

    public class NixPath
    {

        public List<string> Path { get; private set; }
        public bool Absolute { get; set; }
        protected string Combined = "";
        private bool Dirty = true;

        public NixPath(string path = "")
        {
            Absolute = false;
            if (path.Length > 0 && path[0] == '/')
            {
                Absolute = true;
            }
            Path = new List<string>();
            if (path.Length > 0)
            {
                SetPath(path);
            }
        }
        public NixPath(NixPath path)
        {
            Absolute = path.Absolute;
            if (path.Path.Count > 0)
            {
                SetPath(path.ToString());
            }
        }
        public void AppendPath(string path)
        {
            if (path == null || path.Length == 0)
            {
                return;
            }
            if (path[0] == '/')
            {
                Path.Clear();
                Absolute = true;
                Dirty = true;
            }
            string[] split = path.Split('/');
            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].Length == 0)
                {
                    continue;
                }
                if (split[i] == ".")
                {
                    continue;
                }
                else if (split[i] == "..")
                {
                    if (Path.Count > 0)
                    {
                        Path.RemoveAt(Path.Count - 1);
                        Dirty = true;
                    }
                }
                else
                {
                    Path.Add(split[i]);
                    Dirty = true;
                }
            }
        }
        public bool IsRoot()
        {
            return Path.Count == 0;
        }
        public string PopPath()
        {
            if (Path.Count == 0)
            {
                return null;
            }

            string value = Path[Path.Count - 1];
            Path.RemoveAt(Path.Count - 1);
            Dirty = true;
            return value;
        }
        public NixPath Combine(string path)
        {
            NixPath newPath = new NixPath(this.ToString());
            newPath.AppendPath(path);
            Dirty = true;
            return newPath;
        }
        public NixPath Combine(NixPath path)
        {
            NixPath newPath = new NixPath(this.ToString());
            newPath.AppendPath(path.ToString());
            Dirty = true;
            return newPath;
        }
        public void SetPath(string path)
        {
            Path.Clear();
            Absolute = false;
            AppendPath(path);
            Dirty = true;
        }
        override public string ToString()
        {
            if (!Dirty)
            {
                return Combined;
            }

            string result = "";
            if (Absolute)
            {
                result = "/";
            }
            for (int i = 0; i < Path.Count; i++)
            {
                if (i > 0)
                {
                    result += '/';
                }
                result += Path[i];
            }
            Dirty = false;
            Combined = result;
            return result;
        }
    }

}