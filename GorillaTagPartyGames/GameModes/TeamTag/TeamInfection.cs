using System.Collections.Generic;
using System.Linq;
using Fusion;
using GorillaGameModes;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagPartyGames.GameModes.TeamTag;

public class TeamInfection : GorillaGameManager
{
    private Dictionary<int, Team> _playerTeams = new();
    private bool _isRestarting = false;
    private float _restartTimer = 0f;
    private const float RestartDelay = 3f;

    private readonly Dictionary<Team, int> _teamMaterialIndex = new()
    {
        { Team.Teamless, 0 },
        { Team.Red, 2 },
        { Team.Blue, 3 }
    };

    public override GameModeType GameType() => (GameModeType)GameModeInfo.TeamTagId;
    public override string GameModeName() => GameModeInfo.TeamTagGuid;
    public override string GameModeNameRoomLabel() => string.Empty;

    public override void StartPlaying()
    {
        base.StartPlaying();
        if (!NetworkSystem.Instance.IsMasterClient) return;

        ResetGame();
    }

    public override void Tick()
    {
        base.Tick();

        if (_isRestarting)
        {
            _restartTimer += Time.deltaTime;
            if (_restartTimer >= RestartDelay)
            {
                RestartGame();
            }
        }
    }

    public override void ResetGame()
    {
        if (!NetworkSystem.Instance.IsMasterClient) return;

        var players = currentNetPlayerArray.OrderBy(_ => Random.value).ToList();
        for (int i = 0; i < players.Count; i++)
        {
            var team = i switch
            {
                0 => Team.Red,
                1 => Team.Blue,
                _ => Team.Teamless
            };
            ChangePlayerTeam(players[i], team);
        }
    }

    private void RestartGame()
    {
        foreach (var player in currentNetPlayerArray)
        {
            if (player != null)
                ChangePlayerTeam(player, Team.Teamless);
        }

        ResetGame();
        _isRestarting = false;
    }

    public void CheckGameStatus()
    {
        if (!NetworkSystem.Instance.IsMasterClient || _isRestarting) return;

        int total = currentNetPlayerArray.Length;
        if (GetTeamCount(Team.Red) == total ||
            GetTeamCount(Team.Blue) == total ||
            GetTeamCount(Team.Teamless) == total)
        {
            _isRestarting = true;
            _restartTimer = 0f;
        }
    }

    private int GetTeamCount(Team team) =>
        currentNetPlayerArray.Count(player =>
            player != null && _playerTeams.TryGetValue(player.ActorNumber, out var t) && t == team);

    public override void ReportTag(NetPlayer taggedPlayer, NetPlayer taggingPlayer)
    {
        if (!NetworkSystem.Instance.IsMasterClient) return;
        if (!LocalCanTag(taggingPlayer, taggedPlayer)) return;
        if (!_playerTeams.TryGetValue(taggingPlayer.ActorNumber, out var taggingTeam)) return;

        ChangePlayerTeam(taggedPlayer, taggingTeam);
        CheckGameStatus();
    }

    public override bool LocalCanTag(NetPlayer myPlayer, NetPlayer otherPlayer)
    {
        if (myPlayer == null || otherPlayer == null) return false;

        var myTeam = GetPlayerTeam(myPlayer);
        var otherTeam = GetPlayerTeam(otherPlayer);

        return myTeam != Team.Teamless && myTeam != otherTeam;
    }

    private Team GetPlayerTeam(NetPlayer player) =>
        _playerTeams.GetValueOrDefault(player.ActorNumber, Team.Teamless);

    private void ChangePlayerTeam(NetPlayer player, Team newTeam)
    {
        if (player == null) return;

        if (_playerTeams.TryGetValue(player.ActorNumber, out var currentTeam) && currentTeam == newTeam)
            return;

        _playerTeams[player.ActorNumber] = newTeam;

        var rig = FindPlayerVRRig(player);
        if (rig != null)
            UpdatePlayerAppearance(rig);
    }

    public override int MyMatIndex(NetPlayer player)
    {
        var team = GetPlayerTeam(player);
        return _teamMaterialIndex[team];
    }

    public override void OnPlayerLeftRoom(NetPlayer otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        if (!NetworkSystem.Instance.IsMasterClient) return;

        _playerTeams.Remove(otherPlayer.ActorNumber);
        CheckGameStatus();
    }

    public override void NewVRRig(NetPlayer player, int vrrigPhotonViewID, bool didTutorial)
    {
        base.NewVRRig(player, vrrigPhotonViewID, didTutorial);
        if (!NetworkSystem.Instance.IsMasterClient) return;

        if (!_playerTeams.ContainsKey(player.ActorNumber))
            ChangePlayerTeam(player, Team.Teamless);

        CheckGameStatus();
    }

    public override void OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
    {
        if (NetworkSystem.Instance.IsMasterClient) return;

        int size = (int)stream.ReceiveNext();
        for (int i = 0; i < size; i++)
        {
            int actor = (int)stream.ReceiveNext();
            Team team = (Team)(byte)stream.ReceiveNext();

            _playerTeams[actor] = team;
        }
    }

    public override void OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!NetworkSystem.Instance.IsMasterClient) return;

        stream.SendNext(_playerTeams.Count);
        foreach (var (actor, team) in _playerTeams)
        {
            stream.SendNext(actor);
            stream.SendNext((byte)team);
        }
    }

    public override void AddFusionDataBehaviour(NetworkObject behaviour) { }
    public override void OnSerializeRead(object newData) { }
    public override object OnSerializeWrite() => null;
}

public enum Team : byte
{
    Teamless,
    Red,
    Blue
}