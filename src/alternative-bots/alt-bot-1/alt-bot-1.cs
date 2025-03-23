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
        RadarTurnRate = 10;
        TurnRadarRight(Double.PositiveInfinity);
        
        while (IsRunning) {
            //if (RadarTurnRemaining == 0) {
            //    SetTurnRadarRight(1);
            //}
        }
    }


    public override void OnScannedBot(ScannedBotEvent evt) {
        Console.WriteLine(GunDirection);
        Console.WriteLine(GunBearingTo(evt.X, evt.Y));

        SetTurnGunLeft(GunBearingTo(evt.X, evt.Y));
        
        if (Math.Abs(TurnRemaining) < 10) {
            Fire(0.3);
        }
        //Fire(1);
    }


    public override void OnHitByBullet(HitByBulletEvent evt) {
        Console.WriteLine("Kenak peluru njir");
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
