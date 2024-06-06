using Godot;
public static class CreateFighter
{
    public static ClientFighter[] CreateBlankTeam(int length)
    {
        ClientFighter[] team = new ClientFighter[length];
        for (int i = 0; i < length; i++)
        {
            ClientFighter fighterNode = DataGlobals.clientFighterPrefab.Instantiate<ClientFighter>();
            team[i] = fighterNode;
        }
        return team;
    }
    public static ClientFighter[] CreateTeamFromJson(FighterJson[] teamJson)
    {
        ClientFighter[] team = new ClientFighter[teamJson.Length];
        for (int i = 0; i < team.Length; i++)
        {
            ClientFighter fighter = CreateFighterFromJson(teamJson[i]);
            team[i] = fighter;
        }
        return team;
    }
    public static ClientFighter CreateFighterFromJson(FighterJson fighterJson)
    {
        GD.Print($"trying to load fighter with name {fighterJson.Name}");
        FighterData data = DataGlobals.globalFighterDictionary.NameToFighterData[fighterJson.Name];
        if (data == null)
        {
            GD.PrintErr($"couldn't find FighterData with name {fighterJson.Name}");
            return null;
        }
        ActionData[] actions = new ActionData[fighterJson.actionNames.Length];
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i] = LoadAction(fighterJson.actionNames[i]);
        }
        return LoadFighter(data, actions);
    }
    public static ActionData LoadAction(string actionName)
    {
        ActionData data = ChatServer.globalActionDictionary.NameToActionData[actionName];
        if (data == null)
        {
            GD.PrintErr($"couldn't find ActionData with name {actionName}");
            return null;
        }
        return data;
    }
    public static ClientFighter LoadFighter(FighterData data, ActionData[] actions)
    {
        ClientFighter fighterNode = DataGlobals.clientFighterPrefab.Instantiate<ClientFighter>();
        fighterNode.Initialize(data, actions);
        return fighterNode;
    }

}