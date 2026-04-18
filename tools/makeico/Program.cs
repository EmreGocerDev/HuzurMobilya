using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

// Simple tool: convert PNG to ICO (single size)
if (args.Length < 2)
{
    Console.WriteLine("Usage: makeico <input.png> <output.ico>");
    return;
}

var inPath = args[0];
var outPath = args[1];

using var bmp = (Bitmap)Image.FromFile(inPath);
using var ms = new MemoryStream();

// Save as .ico by using Icon.FromHandle
var hicon = bmp.GetHicon();
using var ico = Icon.FromHandle(hicon);
using var fs = new FileStream(outPath, FileMode.Create);
ico.Save(fs);

Console.WriteLine($"Saved {outPath}");
