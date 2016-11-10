﻿using System;
using MonoBrickFirmware.Display;
using MonoBrickFirmware.Movement;
using MonoBrickFirmware.UserInput;
using MonoBrickFirmware.Sensors;
using System.Threading;
using System.Net;
using System.Web;
using System.Collections;
using System.Text;
using System.IO;

namespace Eagle113
{
    class MainClass
    {
        private static readonly object terminateProgram;

        public static void Main(string[] args)
        {
            //AutoFin->false
            ManualResetEvent terminateProgram = new ManualResetEvent(false);
            //Motor_Rady
            LcdConsole.WriteLine("***Motor_Rady***");
            Motor motorA = new Motor(MotorPort.OutA);
            Motor motorD = new Motor(MotorPort.OutD);
            motorA.Off();
            motorD.Off();
            motorA.ResetTacho();
            motorD.ResetTacho();
            //Vehicle_Rady
            Vehicle vehicle = new Vehicle(MotorPort.OutA, MotorPort.OutD);
            WaitHandle waitHandle;
            vehicle.ReverseLeft = false;
            vehicle.ReverseRight = false;
            int b = 0;

            //Sensor_Rady
            LcdConsole.WriteLine("***Sensor_Rady***");
            //Touch_Rady
            var touchSensor1 = new EV3TouchSensor(SensorPort.In1);
            var touchSensor2 = new EV3TouchSensor(SensorPort.In4);
            //UltraSonic_Rady
            var UltraSonicSensor = new EV3UltrasonicSensor(SensorPort.In3, UltraSonicMode.Centimeter);
            //Color_Rady
            EventWaitHandle stopped = new ManualResetEvent(false);
            ColorMode[] modes = { ColorMode.Color, ColorMode.Reflection, ColorMode.Ambient, ColorMode.Blue };
            int modeIdx = 0;
            var sensor = new EV3ColorSensor(SensorPort.In2, ColorMode.Reflection);

            
            //Conection
            LcdConsole.WriteLine("***Conect_Rady***");
            //Http
            Encoding enc = Encoding.UTF8;
            string input = "Detect";
            string url = "http://nursinghomeexplorer.azurewebsites.net/index.php";
            string param = "";
            // 
            Hashtable ht = new Hashtable();
            ht["langpair"] = "#ja";
            ht["hl"] = "en";
            ht["text"] = HttpUtility.UrlEncode(input, enc);
            foreach (string k in ht.Keys)
            {
                param += String.Format("{0}={1}&", k, ht[k]);
            }
            byte[] data = Encoding.ASCII.GetBytes(param);
            // 
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = data.Length;
            LcdConsole.WriteLine("***Conected!***");
            

            //timer set
            System.Timers.Timer aTimer;
            aTimer = new System.Timers.Timer(1000);

            //buts
            ButtonEvents buts = new ButtonEvents();
            LcdConsole.WriteLine("Up read BrightLine");
            LcdConsole.WriteLine("Down read Floor");
            //LcdConsole.WriteLine ("Left Exit program");
            //LcdConsole.WriteLine ("Right Exit program");
            LcdConsole.WriteLine("Enter change color mode");
            LcdConsole.WriteLine("Esc. StartRun");

            buts.UpPressed += () =>
            {
                LcdConsole.WriteLine("Sensor value: " + sensor.ReadAsString());
            };
            buts.DownPressed += () =>
            {
                LcdConsole.WriteLine("Raw sensor value: " + sensor.ReadRaw());
            };
            buts.EnterPressed += () =>
            {
                modeIdx = (modeIdx + 1) % modes.Length;
                sensor.Mode = modes[modeIdx];
                LcdConsole.WriteLine("Sensor mode is set to: " + modes[modeIdx]);
            };
            //Left_Finish
            buts.LeftPressed += () => {
                terminateProgram.Set();
            };

            //Escape_StartRun
            buts.EscapePressed += () =>
            {
                LcdConsole.WriteLine("***StartRun***");
                stopped.Set();
                //loop_run on the line
                while (true)
                {
                           
                    //touchsensor2 Pressed
                    if (touchSensor2.IsPressed() == true)
                    {
                        //motors stop
                        motorA.Brake();
                        motorD.Brake();
                        LcdConsole.WriteLine("***Stop***");

                        Environment.Exit(0);

                        //timer start
                        aTimer.Start();
                        int a = 0;
                        //after 5sec
                        Thread.Sleep(5000);
                        a = int.Parse(aTimer.ToString());
                        if (a >= 5000)
                        {
                            //timer stop
                            a = 0;
                            aTimer.Stop();
                            aTimer.Close();

                            if (touchSensor2.IsPressed() == true)
                            {
                                //
                                Stream reqStream = req.GetRequestStream();
                                reqStream.Write(data, 0, data.Length);
                                reqStream.Close();
                                
                                break;
                            }
                            if (touchSensor1.IsPressed() == true)
                            {
                                //
                                Stream reqStream = req.GetRequestStream();
                                reqStream.Write(data, 0, data.Length);
                                reqStream.Close();
                                
                                break;
                            }

                        }
                    }
                    
                    //touchsensor1 pressed
                    if (touchSensor1.IsPressed() == true)
                    {
                        motorA.Brake();
                        motorD.Brake();
                        LcdConsole.WriteLine("***Stop***");
                        //timer start
                        int a = 0;
                        aTimer.Start();
                        //after 5sec
                        Thread.Sleep(5000);
                        a = int.Parse(aTimer.ToString());
                        if (a >= 5000)
                        {
                            //timer stop
                            a = 0;
                            aTimer.Stop();
                            aTimer.Close();

                            if (touchSensor1.IsPressed() == true)
                            {
                                // 
                                Stream reqStream = req.GetRequestStream();
                                reqStream.Write(data, 0, data.Length);
                                reqStream.Close();
                                break;
                            }
                            if (touchSensor2.IsPressed() == true)
                            {
                                // 
                                Stream reqStream = req.GetRequestStream();
                                reqStream.Write(data, 0, data.Length);
                                reqStream.Close();
                                break;
                            }
                        }
                    }
                    //Ultrasonic on
                    if (UltraSonicSensor.Read() <= 30)
                    {
                        motorA.Brake();
                        motorD.Brake();
                        LcdConsole.WriteLine("***Stop***");
                        //timer start
                        int a = 0;
                        aTimer.Start();
                        //after 5sec
                        Thread.Sleep(5000);
                        a = int.Parse(aTimer.ToString());

                        if (a >= 5000)
                        {
                            //timer stop
                            a = 0;
                            aTimer.Stop();
                            aTimer.Close();

                            if (UltraSonicSensor.Read() >= 30)
                            {
                                // 
                                Stream reqStream = req.GetRequestStream();
                                reqStream.Write(data, 0, data.Length);
                                reqStream.Close();
                                break;
                            }
                        }
                    }


                    b = sensor.Read();

                    if (b < 15)
                    {
                        vehicle.TurnRightForward(10, 100);
                                            }
                    if (15 <= b && b < 60)
                    {
                        vehicle.TurnLeftForward(10, 60);
                    }

                    if (b >= 60)
                    {
                        waitHandle = vehicle.Forward(10, 0, true);
                    }



                    /*
                    if (b > 55)
                    {
                        vehicle.TurnRightForward(10, 45);
                        
                    }
                    if (b > 45)
                    {
                        vehicle.TurnRightForward(10, 15);
                        
                    }

                    if (b > 37)
                    {
                        waitHandle = vehicle.Forward(10, 0, true);
                        
                    }
                    if (b > 30)
                    {
                        vehicle.TurnLeftForward(10, 15);
                        
                    }
                    if (b > 24)
                    {
                        vehicle.TurnLeftForward(10, 50);
                        
                    }
                    if (b > 16)
                    {
                        vehicle.TurnLeftForward(10, 75);
                        
                    }
                    if (b > 0)
                    {
                        waitHandle = vehicle.SpinLeft(10, 85, true);
                        
                    }
                    if (b <= 0)
                    {
                        Stream reqStream = req.GetRequestStream();
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                        break;
                    }*/
                }
            };
            terminateProgram.WaitOne();
            buts.LeftPressed += () => {
                terminateProgram.Set();
            };
        }
            
    }
}
