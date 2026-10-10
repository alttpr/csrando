namespace Randomizer.Games.SuperMetroid.Model;

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
