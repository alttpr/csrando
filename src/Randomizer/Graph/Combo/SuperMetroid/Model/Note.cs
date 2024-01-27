namespace Randomizer.Graph.Combo.SuperMetroid.Model;

using OneOf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal record Note(OneOf<string,List<string>> Content);
internal record DevNote(OneOf<string,List<string>> Content);
