using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace SLAYER_LineOfSight;

public partial class SLAYER_LineOfSight : BasePlugin
{
    public override string ModuleName => "SLAYER_LineOfSight";
    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "SLAYER";
    public override string ModuleDescription => "Check if player 2 is in line of sight of player 1";
    public override void Load(bool hotReload)
    {

    }
    // Check if player 2 is in line of sight of player 1
    [ConsoleCommand("los", "Show Line of Sight")]
	[RequiresPermissions("@css/root")]
	public void ShowEntityCMD(CCSPlayerController? player, CommandInfo command)
	{
        if (player == null || !player.IsValid || player.Connected != PlayerConnectedState.PlayerConnected)return;

        var players = Utilities.GetPlayers().Where(p => p != null && p.IsValid && p.TeamNum > 1 && p.Pawn.Value!.LifeState == (byte)LifeState_t.LIFE_ALIVE).ToList();

        player.PrintToChat($" {ChatColors.DarkRed}[SLAYER LOS] {ChatColors.White}Checking Line of Sight between {ChatColors.Green}'{players[0].PlayerName}' {ChatColors.White}-> {ChatColors.Green}'{players[1].PlayerName}'");

        if(IsPlayerInLOS(players[0], players[1]))
        {
            player.PrintToChat($" {ChatColors.DarkRed}[SLAYER LOS] {ChatColors.White}Player {ChatColors.Green}'{players[1].PlayerName}' {ChatColors.White}is in Line of SIght of player {ChatColors.Green}'{players[0].PlayerName}'");
        }
        else player.PrintToChat($" {ChatColors.DarkRed}[SLAYER LOS] {ChatColors.White}Player {ChatColors.Green}'{players[1].PlayerName}' {ChatColors.White}is {ChatColors.DarkRed}NOT {ChatColors.White}in Line of Sight of player {ChatColors.Green}'{players[0].PlayerName}'");
        
    }
    /// <summary>
    /// Determines if Player2 is in the line of sight (LOS) of Player1.
    /// </summary>
    /// <param name="player1">The first player (the one whose LOS is being checked).</param>
    /// <param name="player2">The second player (the one who might be in the LOS of Player1).</param>
    /// <returns>True if Player1 can see Player2; otherwise, false.</returns>
    private bool IsPlayerInLOS(CCSPlayerController player1, CCSPlayerController player2)
    {
        // Get the forward direction (eye angles) of player 1
        QAngle player1EyeAngles = player1.PlayerPawn.Value!.EyeAngles;

        // Calculate the angle from player 1 to player 2
        QAngle angleToPlayer2 = CalculateAngle(CreateNewVector(player1.PlayerPawn.Value!.AbsOrigin!), CreateNewVector(player2.PlayerPawn.Value!.AbsOrigin!));

        // Check if Player 2 is behind Player 1 by comparing the angles
        if (IsPlayerBehind(player1EyeAngles, angleToPlayer2))
        {
            return false;
        }

        var Position = TraceShape(CreateNewVector(player1.PlayerPawn.Value!.AbsOrigin!), angleToPlayer2, true, true, 3f);
        if(Position != null && System.Numerics.Vector3.Distance(ConvertVectorToVector3(Position), ConvertVectorToVector3(CreateNewVector(player2.PlayerPawn.Value!.AbsOrigin!))) <= 65f)
        {
            return true;
        }
        return false;
    }
    // Function to check if Player 2 is behind Player 1
    private bool IsPlayerBehind(QAngle player1EyeAngles, QAngle player2EyeAngles)
    {
        // Calculate the difference between the two angles (yaw and pitch only)
        float yawDifference = Math.Abs(player1EyeAngles.Y - player2EyeAngles.Y);
        //float pitchDifference = Math.Abs(player1EyeAngles.X - player2EyeAngles.X);

        // Normalize the yaw difference to be within [0, 180] degrees
        if (yawDifference > 180) yawDifference = 360 - yawDifference;

        // If the yaw difference is greater than 52 degrees, Player 2 is behind Player 1
		// Basically this is FOV of Player 1, from my testing 52 is good limit for default FOV
        return yawDifference > 52;
    }
}