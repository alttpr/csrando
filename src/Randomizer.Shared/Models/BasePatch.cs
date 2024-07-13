namespace Randomizer.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BasePatch
{
    public int Id { get; set; }
    public string Version { get; set; }
    public string Commit { get; set; }
    public string Hash { get; set; }
    public byte[] Data { get; set; }
}
