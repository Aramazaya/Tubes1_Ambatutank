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
    private bool FoundMungsuh = false;
    private Dictionary<int, TitikGaya> TitikGaya = new();

    static void Main(string[] args) {
        new NayakaBot().Start();
    }

    NayakaBot() : base(BotInfo.FromFile("alt-bot-1.json")) { }

    /// <summary>
    /// Fungsi bawaan dari Robocode, di sini tempat bot jalan
    /// </summary>
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
            foreach (var Titik in TitikGaya) {
                totalGaya += Titik.Value.Gaya;
            }

            // ngarahin bot ke arah totalGaya
            //SetTurnRightRadians(totalGaya.getDirection() - HeadingRadians);

            if (FoundMungsuh && GunHeat == 0 && Math.Abs(GunTurnRemaining) < 5) {
                SetFire(1);
                FoundMungsuh = false;
            }
            Go();
        }
    }


    public override void OnScannedBot(ScannedBotEvent e) {
        // cek apakah bot yang ketemu itu bot yang udah pernah ketemu atau belum
        if (TitikGaya.ContainsKey(e.ScannedBotId)) {
            // kalo udah ada diupdate
            TitikGaya[e.ScannedBotId] = new TitikGaya(this.X, this.Y, this.Energy, e.X, e.Y, e.Energy);
        }
        else {
            // kalo belum ada, tambahin
            TitikGaya.Add(e.ScannedBotId, new TitikGaya(this.X, this.Y, this.Energy, e.X, e.Y, e.Energy));
        }


        FoundMungsuh = true;
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

    public override void OnBotDeath(BotDeathEvent b) {
        // ngehapus bot yang mati
        TitikGaya.Remove(b.VictimId);
    }

    /// <summary>
    /// Fungsi pembantu
    /// </summary>

    private static double ScaleBulletPower(double distance) {
        double maxPower = 3.0;
        double minDistance = 0;
        double maxDistance = 60;
        return maxPower * (1 - (distance - minDistance) / (maxDistance - minDistance));
    }

}

/// <summary>
/// sorry males bikin file baru
/// di bawah ini isinya class-class pembantu
/// </summary>

// bot buat nyimpen data musuh yang lagi diincer
// ini bahasa jawanya musuh
class MungsuhBot {
        public int ID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Energy { get; set; }
        public double Direction { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }

        public MungsuhBot() {
            this.ID = -1;
            this.X = -1;
            this.Y = -1;
            this.Energy = -1;
            this.Direction = -1;
            this.Speed = -1;
            this.Distance = -1;
        }
        public MungsuhBot(int id, double x, double y, double energy, double direction, double speed, double distance) {
            this.ID = id;
            this.X = x;
            this.Y = y;
            this.Energy = energy;
            this.Direction = direction;
            this.Speed = speed;
            this.Distance = distance;
        }

        public void Reset() {
            this.ID = -1;
            this.X = -1;
            this.Y = -1;
            this.Energy = -1;
            this.Direction = -1;
            this.Speed = -1;
            this.Distance = -1;
        }

        public void Update(ScannedBotEvent e, double BotX, double BotY) {
            this.ID = e.ScannedBotId;
            this.X = e.X;
            this.Y = e.Y;
            this.Energy = e.Energy;
            this.Direction = e.Direction;
            this.Speed = e.Speed;
            this.Distance = Mtk.CalcDistance(this.X, this.Y, BotX, BotY);
        }

        public bool IsEmpty() {
            return this.ID == -1;
        }
    }


class Vektor
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vektor(double length, double Direction) {
        // convert ke radian terus convert ke sistem koordinatnya Robocode (0 derajat tuh ngadep bawah)
        Direction *= Math.PI / 180;
        Direction -= Math.PI / 2;
        // basic trigonometri
        this.X = length * Math.Cos(Direction);
        this.Y = length * Math.Sin(Direction);
    }

    public Vektor(double botX, double botY, double botEnergy, double musuhX, double musuhY, double musuhEnergy) {
        double w1 = 20;
        double w2 = 30;
        double r = Mtk.CalcDistance(botX, botY, musuhX, musuhY);
        double h = botEnergy / musuhEnergy;
        double f = w1 / r + w2 * h;
        double Direction = Mtk.CalcAngle(botX, botY, musuhX, musuhY, false);
        Direction *= Math.PI / 180;
        Direction -= Math.PI / 2;
        this.X = f * Math.Cos(Direction);
        this.Y = f * Math.Sin(Direction);
    }

    public static Vektor operator -(Vektor a) {
        return new Vektor(-a.X, -a.Y);
    }

    public static Vektor operator +(Vektor a, Vektor b) {
        return new Vektor(a.X + b.X, a.Y + b.Y);
    }

    public static Vektor operator -(Vektor a, Vektor b) {
        return new Vektor(a.X - b.X, a.Y - b.Y);
    }

    // ini buat dot product
    public static double operator *(Vektor a, Vektor b) {
        return a.X * b.X + a.Y * b.Y;
    }

    // ini buat scalar product
    public static Vektor operator *(Vektor a, double b)
    {
        return new Vektor(a.X * b, a.Y * b);
    }

    public double GetLength() {
        return Math.Sqrt(this.X * this.X + this.Y * this.Y);
    }

    public double GetDirection() {
        return Math.Atan2(this.Y, this.X) + Math.PI / 2;
    }
}


class TitikGaya {
    public Vektor Gaya { get; set; }
    public TitikGaya(Vektor gaya) {
        this.Gaya = gaya;
    }
    
    public TitikGaya(double botX, double botY, double botEnergy, double musuhX, double musuhY, double musuhEnergy) {
        double w1 = 20;
        double w2 = 30;
        double r = Mtk.CalcDistance(botX, botY, musuhX, musuhY);
        double h = musuhEnergy / botEnergy;
        double f = w1 / r + w2 * h;
        this.Gaya = new Vektor(f, Mtk.CalcAngle(botX, botY, musuhX, musuhY, false));
    }
}


class Mtk
{
    public Mtk() { }
    public static double CalcDistance(double x1, double y1, double x2, double y2) {
        return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }

    public static double CalcAngle(double x1, double y1, double x2, double y2, bool radians) {
        double angle = Math.Atan2(y2 - y1, x2 - x1);
        if (!radians) {
            angle = angle * 180 / Math.PI;
        }
        return angle;
    }
}

