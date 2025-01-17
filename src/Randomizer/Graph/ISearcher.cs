namespace Randomizer.Graph;

using System.Collections.Generic;

public interface ISearcher
{
    IEnumerable<Vertex> GetEmptyLocationsInSet(ItemSetName itemSet, Dictionary<ItemSetName, int>? itemSets = null, bool onlyReachable = true);
    IEnumerable<Vertex> GetVisited();
    bool HasFound(IItem item);
    bool HasVisited(Vertex vertex);
    void ResumeSearch(IEnumerable<Vertex> startAt);


}
