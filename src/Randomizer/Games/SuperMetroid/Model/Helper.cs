namespace Randomizer.Games.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record HelperCollection(HelperCategory[] HelperCategories);

public record Helper
(
    string Name,
    Requirement Requires,
    Note? Note,
    Note? DevNote
);

public record HelperCategory
(
    string Name,
    string Description,
    Helper[] Helpers
);
