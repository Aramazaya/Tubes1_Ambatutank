using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// AramBot
// ------------------------------------------------------------------
// Bot akan melakukan ramming ke arah peluru ketika kena hit, Bot akan menembak dengan power 3 musuh terdekat
// ------------------------------------------------------------------
public class AramBot : Bot
{
    private int perpID = 0;
    private Point closestPoint = null;
    private double closest = double.MaxValue;
    private double closestDir = 0.0;
    private bool hit = false;
    private Point perpDir = null;
    static void Main(string[] args)
    {
        new AramBot().Start();
    }
    AramBot() : base(BotInfo.FromFile("alt-bot-3.json")) { }
    public override void Run()
    {
        BodyColor = Color.Red;
        TracksColor = Color.Cyan;
        TurretColor = Color.Red;
        GunColor = Color.Black;
        RadarColor = Color.Red;
        ScanColor = Color.Yellow;
        BulletColor = Color.Black;
        while (IsRunning){
            if (!hit)           //Ketika Bot tidak sedang melakukan ramming, bot akan menembak musuh terdekat dan diam
            {
                TurnRadarRight(360);
                if (GunHeat == 0)
                {
                    closestDir = DirectionTo(closestPoint.X, closestPoint.Y);
                    double bearing = NormalizeRelativeAngle(closestDir - GunDirection);
                    TurnGunLeft(bearing);
                    Fire(3);
                }
                closest = double.MaxValue;
                closestDir = 0.0;
            }
        }
    }
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        if (!hit){                                              //Ketika Bot tidak sedang ramming, Bot akan mencari scanned enemy terdekat untuk dijadikan target tembakan
            Point p1 = new Point(evt.X, evt.Y);
            Point p2 = new Point(X, Y);
            double eventDistance = FindDistance(p1, p2);
            if (eventDistance < closest){
                closest = eventDistance;
                closestPoint = new Point(evt.X, evt.Y);
            }
        } else {                                                //Ketika Bot terkena Hit, Bot akan melakukan ramming
            if (evt.ScannedBotId == perpID){
                perpDir = new Point(evt.X, evt.Y);
                TurnLeft(NormalizeRelativeAngle(DirectionTo(perpDir.X, perpDir.Y)-Direction));
                Forward(500);
                hit = false;
                perpID = 0;
            }
        }
    }   
    public override void OnHitByBullet(HitByBulletEvent evt)            //Ketika Bot terkena tembakan, Bot akan melakukan Radar search
    {
        perpID = evt.Bullet.OwnerId;
        hit = true;
        TurnRadarRight(360);

    }
    public override void OnHitBot(HitBotEvent botHitBotEvent)           //Ketika Bot mengenai musuh, Bot akan merotasi turret dan menembak ke arah victim bot
    {
        TurnRadarRight(45);
        TurnRadarLeft(90);
        closestDir = DirectionTo(closestPoint.X, closestPoint.Y);
        double bearing = NormalizeRelativeAngle(closestDir - GunDirection);
        TurnGunLeft(bearing);
        if (GunHeat == 0) { Fire(3); }
    }
    public override void OnHitWall(HitWallEvent botHitWallEvent)        //Ketika Bot mengenai wall, lakukan mundur 50 langkah
    {
        Back(50);
    }
    public override void OnDeath(DeathEvent botDeathEvent)
    {
        hit = false;
    }
    static double FindDistance(Point p1, Point p2){
        return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }
}

// ------------------------------------------------------------------
// Communication objects for team messages
// ------------------------------------------------------------------

// Point (x,y) class
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