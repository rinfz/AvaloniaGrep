using System.Collections;
using System.Collections.Generic;

namespace Larus.Models;

public class FileResults
{
    public string Filepath { get; set; }
    public ICollection<FileResult> Results { get; set; }
}
