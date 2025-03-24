using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// AramBot
// ------------------------------------------------------------------
// <DESKRIPSI ALGORITMA GREEDY>
// ------------------------------------------------------------------
public class AramBot : Bot
{
    private int perpID = null;
    private double closest = double.MaxValue;
    private double closestDir = 0.0;
    private bool hit = false;
    private double perpDir = 0.0;

    // The main method starts our bot
    static void Main(string[] args)
    {
        new AramBot().Start();
    }

    // Constructor, which loads the bot config file
    AramBot() : base(BotInfo.FromFile("alt-bot-3.json")) { }

    // Called when a new round is started -> initialize and do some movement
    public override void Run()
    {

        // Set the color of this robot containing the robot colors
        BodyColor = Color.Red;
        TracksColor = Color.Cyan;
        TurretColor = Color.Red;
        GunColor = Color.Black;
        RadarColor = Color.Red;
        ScanColor = Color.Yellow;
        BulletColor = Color.Yellow;

        // Repeat while the bot is running
        while (IsRunning){
            closest = double.MaxValue;
            closestDir = 0.0;
            TurnRadarRight(360);
            if (GunHeat == 0){
                TurnGunRight(NormalizeRelativeAngle(closestDir - GunDirection));
                Fire(3);
            }
        }
    }

    // Called when we scanned a bot -> Send enemy position to teammates
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        if (!hit){
            if (evt.Distance < closest){
                Point p1 = new Point(evt.X, evt.Y);
                Point p2 = new Point(X, Y);
                closest = findDistance(p1, p2);
                closestDir = evt.Direction;
            }
        } else {
            if (evt.ScannedBotId = perpID){
                perpDir = evt.Direction;
                hit = false;
                perpID = null;
                TurnRight(perpDir);
                Ahead(1000);
            }
        }
    }   


    // Called when we have been hit by a bullet -> turn perpendicular to the bullet direction
    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        perpID = evt.bullet.ownerId;
        hit = true;
        TurnRadarRight(360);

    }

    public double findDistance(Point p1, Point p2){
        return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }
    private class Point
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
    }
}
}

// ------------------------------------------------------------------
// Communication objects for team messages
// ------------------------------------------------------------------

// Point (x,y) class

