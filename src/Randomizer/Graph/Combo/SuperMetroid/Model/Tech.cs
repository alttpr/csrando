namespace Randomizer.Graph.Combo.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal record TechCategory(
    string Name,
    string Description,
    List<Tech> Techs
);

internal record Tech(
    string Name,
    LogicalRequirement Requires,
    List<Tech> ExtensionTechs,
    Note Note,
    DevNote DevNote
);

// Placeholder for logical requirements
internal record LogicalRequirement;

internal record SuperMetroidTech(
    List<TechCategory> TechCategories
);
