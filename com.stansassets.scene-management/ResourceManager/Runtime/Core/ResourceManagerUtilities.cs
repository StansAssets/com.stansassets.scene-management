using System.Linq;

namespace StansAssets.ResourceManager
{
    static class ResourceManagerUtilities
    {
        internal static (string groupName, string objectName) SplitObjectPath(string fullName)
        {
            const char splitter = '/';
            
            fullName = fullName.Replace('\\', splitter);

            if (fullName.Contains(splitter))
            {
                return (
                    fullName.Substring(0, fullName.LastIndexOf(splitter)),
                    fullName.Substring(fullName.LastIndexOf(splitter) + 1,
                        fullName.Length - 1 - fullName.LastIndexOf(splitter))
                );
            }

            return (string.Empty, fullName);
        }
    }
}