using System.IO;

static public class PathExtention
{
    static public string Correct(string path)
    {
        #if UNITY_STANDALONE_WIN
            return ToWindowsPath(path);
        #else
            return ToLinuxPath(path);
        #endif
    }

    // 리눅스 경로를 윈도우 경로로 변환
    static public string ToWindowsPath(string linuxPath)
    {
        return linuxPath.Replace("/", "\\");
    }

    // 윈도우 경로를 리눅스 경로로 변환
    static public string ToLinuxPath(string windowsPath)
    {
        return windowsPath.Replace("\\", "/");
    }
}