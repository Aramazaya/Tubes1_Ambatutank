using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// NayakaBot
// ------------------------------------------------------------------
// DESKRIPSI ALGORITMA GREEDY
// ------------------------------------------------------------------
public class NayakaBot : Bot
{
    private const double w1 = 100;
    private const double w2 = 50;
    private const double WallMargin = 30;
    private const int NTitikBantu = 8;

    static void Main(string[] args) {
        new NayakaBot().Start();
    }

    NayakaBot() : base(BotInfo.FromFile("alt-bot-1.json")) { }

    public override void Run() {
        // Ngatur warna
        BodyColor = Color.Red;
        TracksColor = Color.Cyan;
        TurretColor = Color.Red;
        GunColor = Color.Yellow;
        RadarColor = Color.Red;
        ScanColor = Color.Yellow;
        BulletColor = Color.Yellow;

        // Ini buat ngesetting supaya radar, gun, dan body bisa gerak sendiri-sendiri
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;
        AdjustRadarForGunTurn = true;

        while (IsRunning) {
            TurnRadarRight(360);
        }
    }


    public override void OnScannedBot(ScannedBotEvent evt) {
        double angle = evt.Direction;
        if (angle <= 50) {
            TurnGunRight(angle);
            Fire(1);
        }
    }


    public override void OnHitByBullet(HitByBulletEvent evt) {
        double bulletBearing = CalcBearing(evt.Bullet.Direction);
        TurnLeft(90 - bulletBearing);
    }

    public override void OnHitWall(HitWallEvent evt) {
        Console.WriteLine("Nabrak tembok njir");
    }

    public override void OnHitBot(HitBotEvent evt) {
        Console.WriteLine("Nabrak bot lain njir");
    }

}


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
