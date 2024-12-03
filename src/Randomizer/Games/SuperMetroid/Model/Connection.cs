namespace Randomizer.Games.SuperMetroid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public record ConnectionCollection(Connection[] Connections);

public record Connection
(
    string ConnectionType,
    string Direction,
    string Description,
    ConnectionNode[] Nodes,
    Note? Note,
    Note? DevNote
);

public record ConnectionNode
(
    string Area,
    string SubArea,
    int RoomId,
    string RoomName,
    int NodeId,
    string NodeName,
    string Position,
    Note? Note,
    Note? DevNote
);
