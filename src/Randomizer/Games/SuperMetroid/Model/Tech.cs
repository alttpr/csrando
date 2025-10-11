namespace Randomizer.Games.SuperMetroid.Model;
using System.Collections.Generic;

public record TechCategory(
    string Name,
    string Description,
    List<Tech> Techs
);

public record Tech(
    string Name,
    Requirement TechRequires,
    Requirement OtherRequires,
    List<Tech> ExtensionTechs,
    Note? Note,
    Note? DevNote
);

public record TechCollection(
    List<TechCategory> TechCategories
);
