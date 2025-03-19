using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// IanBot
// ------------------------------------------------------------------
// Scan area for enemy. Shoot at enemy depending on distance
// ------------------------------------------------------------------
public class IanBot : Bot
{
    // The main method starts our bot
    static void Main(string[] args)
    {
        new IanBot().Start();
    }

    // Constructor, which loads the bot config file
    IanBot() : base(BotInfo.FromFile("alt-bot-2.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {
        // Prepare robot colors to send to teammates
        var colors = new RobotColors();

        colors.BodyColor = Color.Red;
        colors.TracksColor = Color.Cyan;
        colors.TurretColor = Color.Red;
        colors.GunColor = Color.Yellow;
        colors.RadarColor = Color.Red;
        colors.ScanColor = Color.Yellow;
        colors.BulletColor = Color.Yellow;

        // Set the color of this robot containing the robot colors
        BodyColor = colors.BodyColor;
        TracksColor = colors.TracksColor;
        TurretColor = colors.TurretColor;
        GunColor = colors.GunColor;
        RadarColor = colors.RadarColor;
        ScanColor = colors.ScanColor;
        BulletColor = colors.BulletColor;

        // Send RobotColors object to every member in the team
        BroadcastTeamMessage(colors);

        // Set the radar to turn right forever
        SetTurnRadarRight(Double.PositiveInfinity);

        // Independent Gun and Radar Movement
        setAdjustGunForRobotTurn(true);
        setAdjustRadarForGunTurn(true);

        // Repeat while the bot is running
        // while (IsRunning)
        // {
        //     Forward(100);
        //     TurnGunRight(360);
        //     Back(100);
        //     TurnGunRight(360);
        // }
    }

    // Called when we scanned a bot -> Send enemy position to teammates
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // We scanned a teammate -> ignore
        if (IsTeammate(evt.ScannedBotId))
        {
            return;
        }
        // If scanned an enemy
        double distance = e.getDistance();
        double firePower = Math.Min(500 / distance, 3);
        setTurnGunRight(getHeading() - getGunHeading() + e.getBearing());
        fire(firePower);

        setTurnRight(e.getBearing());
        setAhead(100);
        // Send enemy position to teammates
        // BroadcastTeamMessage(new Point(evt.X, evt.Y));
    }


    // Called when we have been hit by a bullet -> turn perpendicular to the bullet direction
    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Calculate the bullet bearing
        double bulletBearing = CalcBearing(evt.Bullet.Direction);

        // Turn perpendicular to the bullet direction
        TurnLeft(90 - bulletBearing);
    }
}

// ------------------------------------------------------------------
// Communication objects for team messages
// ------------------------------------------------------------------

// Point (x,y) class
class Point
{
    public double X { get; set; }
    public double Y { get; set; }

    public Point(double x, double y)
    {
        X = x;
        Y = y;
    }
}

// Robot colors
class RobotColors
{
    public Color BodyColor { get; set; }
    public Color TracksColor { get; set; }
    public Color TurretColor { get; set; }
    public Color GunColor { get; set; }
    public Color RadarColor { get; set; }
    public Color ScanColor { get; set; }
    public Color BulletColor { get; set; }
}