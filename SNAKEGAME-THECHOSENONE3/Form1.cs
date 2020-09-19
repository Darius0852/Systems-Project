using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;


namespace Snake
{
    public partial class SnakeForm : Form, IMessageFilter
    {
        SnakePlayer Player1;
        FoodManager FoodMngr;
        Random r = new Random();
        int distanceNumber;
         
  
        
        
        private int score = 0;
        public SnakeForm()
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
            this.FormClosed += (s, e) => Application.RemoveMessageFilter(this);
            Player1 = new SnakePlayer(this);
            FoodMngr = new FoodManager(GameCanvas.Width, GameCanvas.Height);
            FoodMngr.AddRandomFood(10);
            ScoreTxtBox.Text = score.ToString();
            

        }

        
        private int scoreNumber = 0;

        public int numColumns
        {
            set
            {
                scoreNumber = Convert.ToInt32(ScoreTxtBox.Text);
            }
        }


        public void ToggleTimer()
        {            
            GameTimer.Enabled = !GameTimer.Enabled;                        
        }

        public void ResetGame()
        {
            serialPort1.Write("f"); // initializes face to middle 
            Thread.Sleep(1000);
            serialPort1.Write("d"); // sends face to rotate to happy face
            Thread.Sleep(1500);
            serialPort1.WriteLine("f"); //rotate back to middle (neutral) face
            Player1 = new SnakePlayer(this);
            FoodMngr = new FoodManager(GameCanvas.Width, GameCanvas.Height);
            FoodMngr.AddRandomFood(10);
            score = 0;
        }

        public bool PreFilterMessage(ref Message msg)
        {
            if (msg.Msg == 0x0101) //KeyUp
                Input.SetKey((Keys)msg.WParam, false);
            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (msg.Msg == 0x100) //KeyDown
                Input.SetKey((Keys)msg.WParam, true);
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void GameCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics canvas = e.Graphics;
            Player1.Draw(canvas);
            FoodMngr.Draw(canvas);
        }

        //check if the snake has hit any of the the four walls it is contained in.
        private void CheckForCollisions()       
        {
            if (Player1.IsIntersectingRect(new Rectangle(-100, 0, 100, GameCanvas.Height)))
                Player1.OnHitWall(Direction.left);

            if (Player1.IsIntersectingRect(new Rectangle(0, -100, GameCanvas.Width, 100)))
                Player1.OnHitWall(Direction.up);

            if (Player1.IsIntersectingRect(new Rectangle(GameCanvas.Width, 0, 100, GameCanvas.Height)))
                Player1.OnHitWall(Direction.right);

            if (Player1.IsIntersectingRect(new Rectangle(0, GameCanvas.Height, GameCanvas.Width, 100)))
                Player1.OnHitWall(Direction.down);

            //Is hitting food
            List<Rectangle> SnakeRects = Player1.GetRects();
            foreach (Rectangle rect in SnakeRects)
            {
                if (FoodMngr.IsIntersectingRect(rect, true))
                {
                    FoodMngr.AddRandomFood();
                    Player1.AddBodySegments(1);
                    score++;
                    ScoreTxtBox.Text = score.ToString();    //update score in textbox
                   
                    serialPort1.Write("f"); // initialises face to middle
                    Thread.Sleep(1000);
                    serialPort1.Write("e"); // sends face to rotate back to happy face
                    Thread.Sleep(1500);
                    serialPort1.WriteLine("f"); //sends face back to middle

                    if (score == 2 || score == 4 || score == 6 || score == 8 || score == 10)
                    //if a score of 2,4,6,8,10 was reached, player would recieve a prize while playing the game
                    {
                        serialPort1.Write("a"); //opens and closes prize motor
                        Thread.Sleep(200);      //slows proces down
                    }   
                                                       
                }
            }
        }


   
        private void distanceSensor()
        {
            int i = 0;      //initialize counter
            while (i < 10)  //counter goes from 0-9
            {
                i++;        //increment counter

                serialPort1.WriteLine("g"); //call for distance sensor data from MBED
                string distData = Convert.ToString(serialPort1.ReadExisting()); //
                Int32.TryParse(distData, out distanceNumber);   //parse the string into an int32 so can use in if statement below
                
                if (distanceNumber > 10)        //if incoming distance sensor integer is less than 10 then execute 
                {
                    MessageBox.Show("STand Closer");
                    Console.WriteLine(distanceNumber);
                } 
                
                Console.WriteLine(distanceNumber);      //if not then just print distnace to console
                Thread.Sleep(500);                      //slow process down by 0.5 seconds


            }
        }

        
        private void SetPlayerMovement()
            
        {
            string colourDirection = Convert.ToString(serialPort1.ReadExisting());
            Console.WriteLine(colourDirection);

            if (colourDirection == "red")       //if incoming string is equal to "red" turn left
            {
                Player1.SetDirection(Direction.left);   
            }
            else if (colourDirection == "green")        //if incoming string is equal to "green" turn right
            {
                Player1.SetDirection(Direction.right);
            }
            else if (colourDirection == "White")        //if incoming string is equal to "White" turn up
            {
                Player1.SetDirection(Direction.up);
            }
            else if (colourDirection == "blue")         //if incoming string is equal to "blue" turn down
            {
                Player1.SetDirection(Direction.down);
            }
            Player1.MovePlayer();

           
        }
        
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            serialPort1.WriteLine("h");         //sets totem head to middle position at the beginning of every game
            SetPlayerMovement();
            CheckForCollisions();
            GameCanvas.Invalidate();
            
           
        }

        private void Start_Btn_Click(object sender, EventArgs e)
        {
            ToggleTimer();
            //BackgroundMethod();
            
        }

        private void DareBtn_Click(object sender, EventArgs e)
        {
            int index = r.Next(4);
            switch (index)
            {
                case 0:
                    MessageBox.Show("Lets make this interesting!");
                    break;
                case 1:
                    MessageBox.Show("If you hit me I will make your life easier!");
                    break;
                case 2:
                    MessageBox.Show("Okay hit me again, sorry I missed it");
                    break;
                case 3:
                    MessageBox.Show("Heres a little bit more food to keep things interesting)");
                    FoodMngr.AddRandomFood(20);
                    GameCanvas.Invalidate();
                    break;
                default:
                    break;
            }
        }



       
        private void SnakeForm_Load(object sender, EventArgs e)
        {
               //used to store the values read                  
            serialPort1.PortName = "COM3";
            //name of port to which device is connected

            serialPort1.BaudRate = 9600;            //set the baudrate
            serialPort1.Parity = Parity.None;       //set parity
            serialPort1.StopBits = StopBits.One;    //set stopbits
            serialPort1.Handshake = Handshake.None; //set handshake
            serialPort1.ReadTimeout = 100;          //set read time
            serialPort1.WriteTimeout = 100;         //set write time

            serialPort1.Open(); //opens the port
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived); 
            //defines new event for every time data received from serial port
            distanceSensor();              
            //calls the distance sensor function to be read 10 times at the beginning
            serialPort1.WriteLine("h"); //calls for colour sensor data from the MBED
            serialPort1.WriteLine("f"); //sets totem head to middle position "neutral face". 
           
        }

        private string DispString;

        //this is the event definition, every time event occurs it refers to this and executes
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (serialPort1.IsOpen)         //while serial port is open
            {
                serialPort1.Write("h");     //sends command to request colour sensor data from MBED                                 
                SetPlayerMovement();        //run this data through movement function                                
                Thread.Sleep(200);          //pause process for 0.2 seconds
                DispString = serialPort1.ReadExisting();    //store current colour in string
             }
        }

        private void DisplayText(object sender, EventArgs e)
        {
         Console.WriteLine(DispString);     //used to print incoming data to console
        }



       



    }
}



    

