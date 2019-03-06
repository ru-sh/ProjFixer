using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ProjFixer
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = args[0];
            var csprojes = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories);
            var relPaths = csprojes.ToDictionary(Path.GetFileName, path => Path.GetRelativePath(root, path));

            foreach (var relPath in relPaths)
            {
                var projFullPath = Path.Combine(root, relPath.Value);
                var projDirName = Path.GetDirectoryName(projFullPath);
                var projRelPath = Path.GetRelativePath(root, projDirName);
                var relPathToRoot = projRelPath.Split('\\', '/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s=> "..\\")
                    .Aggregate((s1, s2) => s1 + s2);

                var xml = XDocument.Parse(File.ReadAllText(projFullPath));
                var itemGroups = xml.Root.Elements("ItemGroup");

                foreach (var itemGroup in itemGroups)
                {
                    var projRef = itemGroup.Elements("ProjectReference").ToList();
                    foreach (var xElement in projRef)
                    {
                        var refProjectIncludeAttr = xElement.Attribute("Include");
                        if (refProjectIncludeAttr != null)
                        {
                            var name = Path.GetFileName(refProjectIncludeAttr.Value);
                            var refProjRealRelativePath = relPaths[name];
                            var relPathToRefProj = Path.Combine(relPathToRoot, refProjRealRelativePath);
                            refProjectIncludeAttr.Value = relPathToRefProj;
                        }
                    }
                    
                }

                xml.Save(projFullPath);
                Console.WriteLine(relPath.Value + " updated.");
            }


            Console.WriteLine("Done. Press Enter.");
            Console.ReadLine();
        }
    }
}