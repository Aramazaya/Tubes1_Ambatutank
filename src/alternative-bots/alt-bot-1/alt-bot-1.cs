using System;
using System.Collections.Generic;
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
    bool foundEnemy = false;
    List<TitikGaya> titikGaya = new List<TitikGaya>();
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
        
        while (IsRunning) {
            TurnRadarRight(360);

            // ngejumlahin semua gaya yang diterima di list titikGaya
            Vektor totalGaya = new Vektor(0, 0);
            for (int i = 0; i < titikGaya.Count; i++) {
                totalGaya += titikGaya[i].gaya;
            }

            // ngarahin bot ke arah totalGaya
            //SetTurnRightRadians(totalGaya.getDirection() - HeadingRadians);

            if (foundEnemy && Math.Abs(GunTurnRemaining) < 5) {
                SetFire(1);
                foundEnemy = false;
            }
            Go();
        }
    }


    public override void OnScannedBot(ScannedBotEvent e) {
        // cek apakah bot yang ketemu itu bot yang udah pernah ketemu atau belum
        bool foundGaya = false;
        for (int i = 0; i < titikGaya.Count; i++) {
            if (titikGaya[i].id == e.ScannedBotId) {
                foundGaya = true;
         // kalo udah pernah ketemu, update gayanya
                titikGaya[i].gaya = new Vektor(this.X, this.Y, this.Energy, e.X, e.Y, e.Energy);
                break;
            }
        }
        // kalo belum pernah ketemu, tambahin ke list
        if (!foundGaya) {
            titikGaya.Add(new TitikGaya(this.X, this.Y, this.Energy, e.ScannedBotId, e.X, e.Y, e.Energy));
        }


        foundEnemy = true;
        SetTurnGunLeft(GunBearingTo(e.X, e.Y));
        //Console.WriteLine(GunBearingTo(evt.X, evt.Y));
        //Console.WriteLine(Math.Atan((evt.X - this.X) / (evt.Y - this.Y)) * (180/Math.PI));
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
class Vektor
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vektor(double length, double direction) {
        // convert ke radian terus convert ke sistem koordinatnya Robocode (0 derajat tuh ngadep bawah)
        direction *= Math.PI / 180;
        direction -= Math.PI / 2;
        // basic trigonometri
        this.X = length * Math.Cos(direction);
        this.Y = length * Math.Sin(direction);
    }

    public Vektor(double botX, double botY, double botEnergy, double musuhX, double musuhY, double musuhEnergy) {
        double w1 = 20;
        double w2 = 30;
        double r = Math.Sqrt(Math.Pow(botX - musuhX, 2) + Math.Pow(botY - musuhY, 2));
        double h = botEnergy / musuhEnergy;
        double f = w1 / r + w2 * h;
        double direction = Math.Atan2(musuhY - botY, musuhX - botX) * 180 / Math.PI;
        direction *= Math.PI / 180;
        direction -= Math.PI / 2;
        this.X = f * Math.Cos(direction);
        this.Y = f * Math.Sin(direction);
    }

    public static Vektor operator -(Vektor a)
    {
        return new Vektor(-a.X, -a.Y);
    }

    public static Vektor operator +(Vektor a, Vektor b)
    {
        return new Vektor(a.X + b.X, a.Y + b.Y);
    }

    public static Vektor operator -(Vektor a, Vektor b)
    {
        return new Vektor(a.X - b.X, a.Y - b.Y);
    }

    // ini buat dot product
    public static double operator *(Vektor a, Vektor b)
    {
        return a.X * b.X + a.Y * b.Y;
    }

    public double getLength()
    {
        return Math.Sqrt(this.X * this.X + this.Y * this.Y);
    }

    public double getDirection()
    {
        // Adjusting the direction by rotating 90 degrees counterclockwise
        return Math.Atan2(this.Y, this.X) + Math.PI / 2;
    }
}

// ini bahasa jawanya musuh
//class Mungsuh {
//    public int id {  get; set; }
//    public double energy { get; set; }
//    public double direction { get; set; }
//    public double speed { get; set; }

//    public Mungsuh(int id, double energy, double direction, double speed) {
//        this.id = id;
//        this.energy = energy;
//        this.direction = direction;
//        this.speed = speed;
//    }
//}

class TitikGaya {
    public int id { get; set; }
    public Vektor gaya { get; set; }
    public TitikGaya(int id, Vektor gaya) {
        this.id = id;
        this.gaya = gaya;
    }
    
    public TitikGaya(double botX, double botY, double botEnergy, int musuhID, double musuhX, double musuhY, double musuhEnergy) {
        double w1 = 20;
        double w2 = 30;
        double r = Math.Sqrt(Math.Pow(botX - musuhX, 2) + Math.Pow(botY - musuhY, 2));
        double h = musuhEnergy / botEnergy;
        double f = w1 / r + w2 * h;
        this.id = musuhID;
        this.gaya = new Vektor(f, Math.Atan2(musuhY - botY, musuhX - botX) * 180 / Math.PI);
    }
}