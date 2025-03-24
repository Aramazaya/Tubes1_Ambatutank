using System;
using System.Collections.Generic;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// ------------------------------------------------------------------
// AmbatuTank
// ------------------------------------------------------------------
// <DESKRIPSI ALGORITMA GREEDY>
// ------------------------------------------------------------------
public class AmbatuTank : Bot
{
    private Dictionary<int, TitikGaya> TitikGaya = new();
    private MungsuhBot Mungsuh = new();
    // The main method starts our bot
    static void Main(string[] args)
    {
        new AmbatuTank().Start();
    }

    // Constructor, which loads the bot config file
    AmbatuTank() : base(BotInfo.FromFile("main-bot.json")) { }

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

        // Repeat while the bot is running
        while (IsRunning) {
            TurnRadarRight(360);

            // ngejumlahin semua gaya yang diterima di list titikGaya
            Vektor totalGaya = new Vektor(0, 0);
            foreach (var Titik in TitikGaya) {
                totalGaya += Titik.Value.Gaya;
            }
            Tembak();
            Go();
        }
    }

    // Called when we scanned a bot -> Send enemy position to teammates
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


        // update mungsuh
        if (Mungsuh.IsEmpty() || e.Energy > Mungsuh.Energy || e.ScannedBotId == Mungsuh.ID) {
            Mungsuh.Update(e, this.X, this.Y);
        } 

        //Console.WriteLine(GunBearingTo(evt.X, evt.Y));
        //Console.WriteLine(Math.Atan((evt.X - this.X) / (evt.Y - this.Y)) * (180/Math.PI));
    }

    public override void OnHitWall(HitWallEvent evt) {
        SetTurnLeft(180);
    }

    public override void OnBotDeath(BotDeathEvent b) {
        // ngehapus bot yang mati
        TitikGaya.Remove(b.VictimId);
        // ngereset data musuh kalo yang mati itu musuh yang lagi diincer
        if (Mungsuh.ID == b.VictimId) {
            Mungsuh.Reset();
        }
    }

    private void Tembak() {
        if (!Mungsuh.IsEmpty() && GunHeat == 0 && Math.Abs(GunTurnRemaining) < 10) {
            double enemyVelocity = Mungsuh.Speed;
            double enemyHeading = Mungsuh.Direction * Math.PI / 180;  // convert ke radians
            double bulletSpeed = 20 - 3 * ScaleBulletPower(Mtk.CalcDistance(this.X, this.Y, Mungsuh.X, Mungsuh.Y));
            double distance = Mungsuh.Distance;
            double bulletPower = ScaleBulletPower(distance);
            Console.WriteLine(bulletPower);
            Titik t = PredictEnemyPosition();
            SetTurnGunLeft(GunBearingTo(t.X, t.Y));
            SetFire(bulletPower);
        }
        MoveToEnemy(Mungsuh.X, Mungsuh.Y);
        return;
    }

    private static double ScaleBulletPower(double distance) {
        double maxPower = 3.0;
        double maxDistance = 1000;
        if (distance > maxDistance) {
            distance = maxDistance-100;
        }
        return maxPower * (1 - distance / maxDistance);
    }

    private Titik PredictEnemyPosition() {
        double EnemyDistance = Mtk.CalcDistance(this.X, this.Y, Mungsuh.X, Mungsuh.Y);
        double BulletSpeed = 20 - 3 * ScaleBulletPower(EnemyDistance);
        double t = EnemyDistance / BulletSpeed + 5;
        double enemyVelocity = Mungsuh.Speed;
        double enemyHeading = Mungsuh.Direction;
        double predictedX = Mungsuh.X + enemyVelocity * Math.Cos(enemyHeading * Math.PI/180) * t;
        double predictedY = Mungsuh.Y + enemyVelocity * Math.Sin(enemyHeading * Math.PI/180) * t;
        return new Titik(predictedX, predictedY);
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

class Titik {
    public double X { get; set; }
    public double Y { get; set; }
    public Titik(double x, double y) {
        this.X = x;
        this.Y = y;
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