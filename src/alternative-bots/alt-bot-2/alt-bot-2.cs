using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// IanBot
// ------------------------------------------------------------------
// Scan area for enemy. Shoot at enemy with the lowest energy
// ------------------------------------------------------------------
public class IanBot : Bot
{
    private int weakestEnemy = -1;
    private double weakestEnergy = double.MaxValue;
    private double weakestEnemyX, weakestEnemyY;
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

        colors.BodyColor = Color.DarkTurquoise;
        colors.TracksColor = Color.SeaShell;
        colors.TurretColor = Color.MintCream;
        colors.GunColor = Color.MintCream;
        colors.RadarColor = Color.SkyBlue;
        colors.ScanColor = Color.LightSkyBlue;
        colors.BulletColor = Color.LimeGreen;

        // Set the color of this robot containing the robot colors
        BodyColor = colors.BodyColor;
        TracksColor = colors.TracksColor;
        TurretColor = colors.TurretColor;
        GunColor = colors.GunColor;
        RadarColor = colors.RadarColor;
        ScanColor = colors.ScanColor;
        BulletColor = colors.BulletColor;


        // Repeat while the bot is running
        while (IsRunning)
        {
            TurnRadarRight(360);
            FireAtWill();
        }
    }

    // Called when we scanned a bot -> Send enemy position to teammates
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        if (evt.Energy < weakestEnergy)
        {
            weakestEnergy = evt.Energy;
            weakestEnemy = evt.ScannedBotId;
            weakestEnemyX = evt.X;
            weakestEnemyY = evt.Y;
        }
    }


    // Called when we have been hit by a bullet -> turn perpendicular to the bullet direction
    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Calculate the bullet bearing
        double bulletBearing = CalcBearing(evt.Bullet.Direction);
        TurnRight(NormalizeRelativeAngle(90 - bulletBearing)); // Perpendicular move
        Back(50); // Move to evade further shots
    }

    public override void OnBotDeath(BotDeathEvent evt)
    {
        if (evt.VictimId == weakestEnemy)
        {
            weakestEnemy = -1;
            weakestEnergy = double.MaxValue;
        }
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        if (evt.Energy < weakestEnergy)
        {
            SetTurnRight(NormalizeRelativeAngle(DirectionTo(evt.X, evt.Y) - Direction));
            Forward(100); // RAM THEM!!
        }
    }


    private void FireAtWill()
    {
        if (weakestEnemy != -1)
        {
            double bearing = DirectionTo(weakestEnemyX, weakestEnemyY); 
            double gunBearing = NormalizeRelativeAngle(bearing-GunDirection); 
            //TurnGunRight(gunBearing);
            TurnGunLeft(gunBearing);
            if (DistanceTo(weakestEnemyX, weakestEnemyY) < 50)
            {
                Fire(3);
            }
            else
            {
                Fire(1);
            }
            MoveToEnemy(weakestEnemyX, weakestEnemyY);
            weakestEnemy = -1;
            weakestEnergy = double.MaxValue;
        }
    }

    private void MoveToEnemy(double targetX, double targetY)
    {
        double angleToEnemy = DirectionTo(targetX, targetY);
        TurnLeft(NormalizeRelativeAngle(angleToEnemy-Direction));
        double offset = 45;
        SetForward(150);
        // for (int i = 0; i < 3; i++)
        // {
        SetTurnRight(offset);
        WaitFor(new TurnCompleteCondition(this));
        SetTurnLeft(2 * offset);
        WaitFor(new TurnCompleteCondition(this));
        SetTurnRight(offset);
        // }
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

public class TurnCompleteCondition : Condition
{
    private readonly Bot bot;

    public TurnCompleteCondition(Bot bot)
    {
        this.bot = bot;
    }

    public override bool Test()
    {
        // turn is complete when the remainder of the turn is zero
        return bot.TurnRemaining == 0;
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